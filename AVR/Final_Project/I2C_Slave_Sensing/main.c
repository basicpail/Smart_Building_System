/*
* My_Slave.c
*
* Created: 2020-09-10 오후 1:32:44
* Author : PKNU
*/

#include <avr/io.h>
#include <util/twi.h>
#include <avr/interrupt.h>
#include <stdlib.h>
#include <string.h>
#include <stdio.h>
#include <util/delay.h>

#define I2C_ADDR 0x14 //슬레이브 주소.
#define CR 0x0D       //i2c 통신 문자열 마지막 문자.

#define Floor1_Data 0x20
#define Floor2_Data 0x21
#define Floor3_Data 0x22
#define Fire_Data 0x23
#define Water_Data 0x24
#define Photo_Data 0x25

//========================온습도 센서 포트 설정
#define DHT22_DDR DDRA
#define DHT22_PORT PORTA
#define DHT22_PIN PINA
#define Floor1 PA1
#define Floor2 PA2
#define Floor3 PA3
//==============================================

//==========================ADC 센서 포트 설정
#define Water_Pin 0x00 //PF0
#define Photo_Pin 0x01 //PF1
#define Fire_Pin  0x02 //PF2
//===========================================

#define True 1
#define False 0

int First_Request=True;

//===========================i2c 통신에서 보낼 문자열
char String_Temp_Humi1[16];
char String_Temp_Humi2[16];
char String_Temp_Humi3[16];
char String_Water[16];
char String_Photo[16];
char String_Fire[16];
char *String_Pt;
//===================================================

//==============================센서 값 변수
float Temp_Data1, Humi_Data1;
float Temp_Data2, Humi_Data2;
float Temp_Data3, Humi_Data3;
//================================================

uint16_t PhotoData = 0;
uint16_t WaterData = 0;
uint16_t FireData = 0;



char I2C_Cmd;      //write,read의 첫 번째 Data
char I2C_State;    //write,read 때 들어온 Data 순서 변수.
char I2C_In_Data;  //마스터에서 슬레이브로 들어온 Data
char I2C_Out_Data; //슬레이브에서 마스터로 보낸 Data

void Initialize();
void DHT22_GetData(float *temperature, float *humidity, uint8_t DHT22_INPUTPIN);
void Temp_Humi_Get_Data();
void Out_Data_Set(char String_Temp_Humi[]);
void ADC_Get_Data(uint8_t Sensor_Pin,uint16_t Sensor_Data,char String_SensorData[]);

int main(void)
{
	Initialize(); //ADC,I2C 레지스터 초기화
	while (1)
	{
		Temp_Humi_Get_Data();
		ADC_Get_Data(Water_Pin,WaterData,String_Water);
		ADC_Get_Data(Fire_Pin,FireData,String_Fire);
		ADC_Get_Data(Photo_Pin,PhotoData,String_Photo);
		
	}
}

ISR(TWI_vect) //I2C 인터럽트
{
	cli();
	switch(TW_STATUS)
	{
		
		case TW_SR_SLA_ACK: // SLA+W received, ACK returned.
		I2C_State = 0;  // Slave receive address 가 정상으로 수신된 상태
		break;

		// Data received, ACK returned.
		case TW_SR_DATA_ACK:
		I2C_State++;
		
		if(I2C_State == 1) // 첫 Data를 정상으로 수신한 상태. 이 Data는 Command 또는 Data로 사용할 수 있다.
		{
			I2C_Cmd = TWDR;

			if(I2C_Cmd == Floor1_Data)
			{
				Out_Data_Set(String_Temp_Humi1);
			}
			else if(I2C_Cmd == Floor2_Data)
			{
				Out_Data_Set(String_Temp_Humi2);
			}
			else if(I2C_Cmd == Floor3_Data)
			{
				Out_Data_Set(String_Temp_Humi3);
			}
			else if(I2C_Cmd == Fire_Data)
			{
				Out_Data_Set(String_Fire);
			}
			else if(I2C_Cmd == Water_Data)
			{
				Out_Data_Set(String_Water);
			}
			else if(I2C_Cmd == Photo_Data)
			{
				Out_Data_Set(String_Photo);
			}

			
		}
		
		if(I2C_State == 2)
		{
			// write_byte_data(int addr,char cmd,char val) 명령의 두번째 Data로,
			// 이 Data는 Input buffer에 저장 한다.
			if(I2C_Cmd == 0xa0)
			{
				I2C_In_Data = TWDR;
			}
		}
		
		break;

		case TW_SR_STOP: // I2C 통신 Stop상태
		I2C_State = 0;
		break;
		
		case TW_ST_SLA_ACK: // SLA+R received, ACK returned.
		TWDR = I2C_Out_Data;
		break;
		
		case TW_ST_DATA_ACK: // Data transmitted, ACK received.
		TWDR = I2C_Out_Data;
		break;

		default:
		break;
	}
	// Set TWIE (TWI Interrupt enable), TWEN (TWI Enable),
	// TWEA (TWI Enable Acknowledge), TWINT (Clear TWINT flag by writing a 1)
	TWCR = 0xC5;
	sei();
}

void Initialize()
{
	cli(); //전역 인터럽트 비활성화.
	
	//ADC 초기화
	ADMUX =  0x00; //0000 0000             REFS1 REFS0 ADLAR MUX4 \ MUX2 MUX2 MUX1 MUX0  기준전압 AREF(3.3v) 사용
	ADCSRA = 0x87; //1000 0111             ADEN ADSC ADFR ADIF \ ADIE ADPS2 ADPS1 ADPS0
	
	//I2C 초기화
	TWAR = I2C_ADDR << 1; // TWI Address Register 설정.
	TWCR = (1<<TWIE) | (1<<TWEA) | (1<<TWINT) | (1<<TWEN); //  Enable TWI, Clear TWINT, Enable TWI Interrupt
	
	sei(); //전역 인터럽트 활성화.
}

void ADC_Get_Data(uint8_t Sensor_Pin,uint16_t Sensor_Data,char String_SensorData[])
{
	ADMUX = Sensor_Pin;
	ADCSRA |=0x40; //ADC 시작
	while((ADCSRA & 0x10) == 0x00); //ADC 변환까지 대기
	Sensor_Data = ADC;
	if(Sensor_Pin == Fire_Pin)
	{
		if(Sensor_Data < 600)
		{
			sprintf(String_SensorData,"On");
		}
		else
		{
			sprintf(String_SensorData,"Off");
		}
	}
	else if(Sensor_Pin == Water_Pin)
	{
		if(Sensor_Data > 400)
		{
			sprintf(String_SensorData,"On");
		}
		else
		{
			sprintf(String_SensorData,"Off");
		}
		//sprintf(String_SensorData,"%d",Sensor_Data);
	}
	else if(Sensor_Pin == Photo_Pin)
	{
		if(Sensor_Data > 200)
		{
			sprintf(String_SensorData,"On");
		}
		else
		{
			sprintf(String_SensorData,"Off");
		}
	}
	
}

void Out_Data_Set(char String_Temp_Humi[])
{
	if(First_Request==True)
	{
		String_Pt=&String_Temp_Humi[0];
		First_Request=False;
	}
	
	I2C_Out_Data=*String_Pt;
	String_Pt++;
	
	if(I2C_Out_Data==0)
	{
		First_Request=True;
		I2C_Out_Data = CR;
	}
}


void Temp_Humi_Get_Data()
{
	DHT22_GetData(&Temp_Data1,&Humi_Data1,Floor1);
	DHT22_GetData(&Temp_Data2,&Humi_Data2,Floor2);
	DHT22_GetData(&Temp_Data2,&Humi_Data2,Floor3);
}

void DHT22_GetData(float *temperature, float *humidity,uint8_t DHT22_INPUTPIN)
{
	//DHT22 DataSheet의 타이밍도에 따라 작성함.

	uint8_t bits[5];
	uint8_t i,j = 0;
	char Temp_buf[8];
	char Humi_buf[8];
	
	memset(bits, 0, sizeof(bits));
	
	//DHT22 센서 Port 리셋
	DHT22_DDR |= (1<<DHT22_INPUTPIN);  // DHT22 핀 Output 설정
	DHT22_PORT |= (1<<DHT22_INPUTPIN); // High
	_delay_ms(100);
	
	//Send Request
	DHT22_PORT &= ~(1<<DHT22_INPUTPIN); //Low
	_delay_us(500);
	DHT22_PORT |= (1<<DHT22_INPUTPIN); //High
	DHT22_DDR &= ~(1<<DHT22_INPUTPIN); //Input
	_delay_us(40);
	
	
	//check start condition 1
	if((DHT22_PIN & (1<<DHT22_INPUTPIN)))
	{ //DHT22 PIN == High 이면
		if(DHT22_INPUTPIN==Floor1)
		{
			sprintf(String_Temp_Humi1,"Error1"); //온습도를 '/'기준으로 문자열 생성.
		}
		else if(DHT22_INPUTPIN==Floor2)
		{
			sprintf(String_Temp_Humi1,"Error2"); //온습도를 '/'기준으로 문자열 생성.
		}
		else if(DHT22_INPUTPIN==Floor3)
		{
			sprintf(String_Temp_Humi1,"Error3"); //온습도를 '/'기준으로 문자열 생성.
		}
		return ;
	}
	_delay_us(80);
	//check start condition 2
	if(!(DHT22_PIN & (1<<DHT22_INPUTPIN))) {
		if(DHT22_INPUTPIN==Floor1)
		{
			sprintf(String_Temp_Humi1,"Error1"); //온습도를 '/'기준으로 문자열 생성.
		}
		else if(DHT22_INPUTPIN==Floor2)
		{
			sprintf(String_Temp_Humi1,"Error2"); //온습도를 '/'기준으로 문자열 생성.
		}
		else if(DHT22_INPUTPIN==Floor3)
		{
			sprintf(String_Temp_Humi1,"Error3"); //온습도를 '/'기준으로 문자열 생성.
		}
		return ;
	}
	_delay_us(80);
	
	//Data을 읽음
	for (j=0; j<5; j++) { //5 byte 데이터 읽음
		uint8_t result=0;
		for(i=0; i<8; i++)
		{
			while(!(DHT22_PIN & (1<<DHT22_INPUTPIN))); // High 레벨을 기다림.
			_delay_us(30);
			if(DHT22_PIN & (1<<DHT22_INPUTPIN)) //30us 후 High 이면 '1' 읽기.
			result |= (1<<(7-i));
			while(DHT22_PIN & (1<<DHT22_INPUTPIN)); //Low 까지 기다림.
		}
		bits[j] = result;
	}
	
	//Port 리셋
	DHT22_DDR |= (1<<DHT22_INPUTPIN); //Output
	DHT22_PORT |= (1<<DHT22_INPUTPIN); //High
	_delay_ms(100);
	
	if ((uint8_t)(bits[0] + bits[1] + bits[2] + bits[3]) == bits[4]) //Checksum과 들어온 Data 확인
	{
		*temperature = (float)(bits[2]<<8 | bits[3])/10.0;
		*humidity = (float)(bits[0]<<8 | bits[1])/10.0;
		
		if(DHT22_INPUTPIN==Floor1)
		{
			dtostrf(*temperature, 3, 1, Temp_buf); //실수를 문자열도 만든다.
			dtostrf(*humidity, 3, 1, Humi_buf);    //실수를 문자열도 만든다.
			sprintf(String_Temp_Humi1,"%s/%s",Temp_buf,Humi_buf); //온습도를 '/'기준으로 문자열 생성.
		}
		else if(DHT22_INPUTPIN==Floor2)
		{
			dtostrf(*temperature, 3, 1, Temp_buf);
			dtostrf(*humidity, 3, 1, Humi_buf);
			sprintf(String_Temp_Humi2,"%s/%s",Temp_buf,Humi_buf);
		}
		else if(DHT22_INPUTPIN==Floor3)
		{
			dtostrf(*temperature, 3, 1, Temp_buf);
			dtostrf(*humidity, 3, 1, Humi_buf);
			sprintf(String_Temp_Humi3,"%s/%s",Temp_buf,Humi_buf);
		}
		return;
	}
	return ;
}





