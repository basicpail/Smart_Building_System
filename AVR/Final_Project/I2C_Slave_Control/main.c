/*
* I2C_Slave_Control.c
*
* Created: 2020-09-16 오전 9:51:00
* Author : PKNU
*/

#include <avr/io.h>
#include <util/twi.h>
#include <avr/interrupt.h>
#include <stdlib.h>
#include <string.h>
#include <stdio.h>
#include <util/delay.h>

#define I2C_ADDR 0x15
#define CR 0x0D
#define Temp 0
#define Humi 1
#define ONLED PORTF
#define PAN PORTA
#define FIREBUZZER PORTB
#define PUMP PORTC
#define FIRELED PORTE
#define Raser PORTD
#define ON 1
#define OFF 0


char i2c_cmd, i2c_in_data, i2c_out_data;
char i2c_state;
char *pt_out;

int On_Off_Flag=0;

void LEDControl();
void PANControl();
void PUMPControl();
void ONFIRE();
void Raser_On();
void Raser_Off();
void DETECTION();

int main(void)
{
	DDRF=0xFF; //LED
	DDRA=0xFF; //PAN
	DDRB=0xFF; //FIREBUZZER
	DDRC=0xFF; //PUMP
	DDRD |= 0x04;
	DDRE=0xFF;
	
	cli();

	// load address into TWI address register
	TWAR = I2C_ADDR << 1;
	// set the TWCR to enable address matching and enable TWI, clear TWINT, enable TWI interrupt
	TWCR = (1<<TWIE) | (1<<TWEA) | (1<<TWINT) | (1<<TWEN);
	
	sei();


	/* Replace with your application code */
	while (1)
	{
		if ((i2c_in_data == 'q')|(i2c_in_data == 'w')|(i2c_in_data == 'e')|(i2c_in_data == 'z')|(i2c_in_data == 'x')|(i2c_in_data == 'c'))
		{
			LEDControl();
		}
		else if ((i2c_in_data == 'r')|(i2c_in_data == 'v'))
		{
			PANControl();
		}
		else if ((i2c_in_data == 't')|(i2c_in_data == 'b'))
		{
			PUMPControl();
		}
		else if ((i2c_in_data == 'y')|(i2c_in_data == 'n'))
		{
			ONFIRE();
		}
		else if((i2c_in_data == 'u'))
		{
			Raser_On();
		}
		else if((i2c_in_data == 'm'))
		{
			Raser_Off();
		}
		else if((i2c_in_data == 'o')|(i2c_in_data == 'p'))
		{
			DETECTION();
		}
		
	}
}

void LEDControl()
{
	//1층 LED ON
	if (i2c_in_data == 'q')
	{
		ONLED = 0x01 | ONLED; //PF0
	}
	//2층 LED ON
	else if (i2c_in_data == 'w')
	{
		ONLED = 0x04 | ONLED; //PF2
	}
	//3층 LED ON
	else if (i2c_in_data == 'e')
	{
		ONLED = 0x10 | ONLED; //PF4
	}
	//1층 LED OFF
	else if (i2c_in_data == 'z')
	{
		ONLED = ONLED &~(0x01);
	}
	//2층 LED OFF
	else if (i2c_in_data == 'x')
	{
		ONLED = ONLED &~(0x04);
	}
	//3층 LED OFF
	else if (i2c_in_data == 'c')
	{
		ONLED = ONLED &~(0x10);
	}
}

void PANControl()
{
	//PAN ON
	if(i2c_in_data == 'r')
	{
		PAN = 0x01; //PA0
	}
	//PAN OFF
	else if(i2c_in_data == 'v')
	{
		PAN = 0x00;
	}
}

void PUMPControl()
{
	//PUMP ON
	if(i2c_in_data == 't')
	{
		PUMP = 0x01; //PC0
	}
	//PUMP OFF
	else if(i2c_in_data == 'b')
	{
		PUMP = 0x00;
	}
}

void ONFIRE()
{
	if (i2c_in_data == 'y')
	{
		ONLED = 0x40; //PF6
		PAN = 0x00;
		
		TCCR0 = 0x1F; //0001 1111
		OCR0 = 9;
		_delay_ms(100);
		
		for(int i=35;i>8;i--)
		{
			if(i2c_in_data !='y')
			{
				break;
			}
			OCR0=i;
			_delay_ms(10);
			if(i==9) _delay_ms(500);
		}
		for(int i=8;i<35;i++)
		{
			if(i2c_in_data !='y')
			{
				break;
			}
			OCR0=i;
			_delay_ms(10);
			if(i==25) _delay_ms(500);
		}
	}
	else if (i2c_in_data == 'n')
	{
		//DDRB = 0x00;
		TCCR0 = 0x0F;
		ONLED = 0x00;
	}
}

void DETECTION()
{
	if (i2c_in_data == 'o')
	{
		TCCR0 = 0x1F; //0001 1111
		OCR0 = 9;
		_delay_ms(100);
		
		for(int i=35;i>8;i--)
		{
			if(i2c_in_data !='o')
			{
				break;
			}
			OCR0=i;
			_delay_ms(10);
			if(i==9) _delay_ms(500);
		}
		for(int i=8;i<35;i++)
		{
			if(i2c_in_data !='o')
			{
				break;
			}
			OCR0=i;
			_delay_ms(10);
			if(i==25) _delay_ms(500);
		}
	}
	else if (i2c_in_data == 'p')
	{
		TCCR0 = 0x0F;
	}
}

void Raser_On()
{
	Raser |= 0x04; //0000 0100
	On_Off_Flag=1;
	
}
void Raser_Off()
{
	Raser = 0x00; //1111 1011
	On_Off_Flag=0;
}


ISR(TWI_vect)
{
	cli();
	// TWSR Rg의 상태에 따라 다른 처리를 한다.
	switch(TW_STATUS)
	{
		
		case TW_SR_SLA_ACK:			// 0x60
		i2c_state = 0;
		break;
		
		// Data received, ACK returned.
		case TW_SR_DATA_ACK:		// 0x80
		i2c_state++;
		
		if(i2c_state == 1) // 첫 Data를 정상으로 수신한 상태. 이 Data는 Command 또는 Data로 사용할 수 있다.
		{
			i2c_cmd = TWDR;
			
		}
		
		if(i2c_state == 2)
		{
			// write_byte_data(int addr,char cmd,char val) 명령의 두번째 Data로,
			// 이 Data는 Input buffer에 저장 한다.
			if(i2c_cmd == 0xa0)
			{
				i2c_in_data = TWDR;
				
			}
		}
		break;
		// Stop or repeated start condition received while selected.
		case TW_SR_STOP:			// 0xA0
		i2c_state = 0;
		break;
		
		// SLA+R received, ACK returned.
		case TW_ST_SLA_ACK:			// 0xA8
		TWDR = i2c_out_data;
		break;
		
		// Data transmitted, ACK received.
		case TW_ST_DATA_ACK:		// 0xB8
		TWDR = i2c_out_data;
		break;
		
		default:
		break;
	}
	// Set TWIE (TWI Interrupt enable), TWEN (TWI Enable),
	// TWEA (TWI Enable Acknowledge), TWINT (Clear TWINT flag by writing a 1)
	TWCR = 0xC5;
	sei();
}