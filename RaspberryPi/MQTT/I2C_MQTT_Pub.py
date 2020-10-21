#!/usr/bin/env python
import paho.mqtt.client as mqtt
import json 
import time
import datetime as dt
import uuid
import smbus
from collections import OrderedDict

count = 0
AVR_1_ADDRESS = 0x014
Floor1_Data = 0x20
Floor2_Data = 0x21
Floor3_Data = 0x22
Fire_Data = 0x23
Water_Data = 0x24
Photo_Data = 0x25

I2Cbus = smbus.SMBus(1)

SlaveAddress = AVR_1_ADDRESS
dev_id = "TKKim"
#mqtt publisher
broker_address = "210.119.12.77"
client2 = mqtt.Client(dev_id)
client2.connect(broker_address)#접속

def I2C_Read_data(Data_Name):    
    out_str=''
    for i in range(16):
        in_data = I2Cbus.read_byte_data(SlaveAddress, Data_Name)
        if in_data == 13: 
            break
        out_str += chr(in_data)
        time.sleep(0.01)
    
    return out_str
    
#dh11 init
while True:
    count += 1        
    currtime = dt.datetime.now().strftime('%Y-%m-%d %H:%M:%S') # 현재 time 생성
    
    # groupdata 만들기
    raw_data = OrderedDict() #데이터 생성
    raw_data["Curr_time"] = currtime
    
    try:
        raw_data["Fire"] = I2C_Read_data(Fire_Data)
        
        raw_data["Water"] = I2C_Read_data(Water_Data)
        
        raw_data["Photo"] = I2C_Read_data(Photo_Data)
        
        
        split_TempHumi1 = I2C_Read_data(Floor1_Data).split('/')
        raw_data["firsttemp"] = split_TempHumi1[0]
        raw_data["firsthumid"] = split_TempHumi1[1]

        split_TempHumi2 = I2C_Read_data(Floor2_Data).split('/')
        #split_TempHumi2 = I2C_Read_data(Floor2_Data)
        #print(split_TempHumi2)
        raw_data["secondtemp"] = split_TempHumi2[0]
        raw_data["secondhumid"] = split_TempHumi2[1]

        split_TempHumi3 = I2C_Read_data(Floor3_Data).split('/')
        raw_data["thirdtemp"] = split_TempHumi3[0]
        raw_data["thirdhumid"] = split_TempHumi3[1]
        
        
               
        pub_data = json.dumps(raw_data, ensure_ascii=False, indent="\t") #데이터 json으로 변환
        #mqtt published
        print(count, pub_data)
        client2.publish("Temp_Humid", pub_data) # 데이터 보내기
        
        time.sleep(1)
    except Exception as ex:
        print('Error raised : ',ex)


