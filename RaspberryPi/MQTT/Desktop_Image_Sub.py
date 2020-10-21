import paho.mqtt.client as mqtt
import matplotlib.pyplot as plt
import time
import json
import pymysql
import pandas as pd
from sqlalchemy import create_engine
from PIL import Image
import base64
from io import BytesIO




MQTT_SERVER = '210.119.12.77'
MQTT_PATH = "common"
MQTT_PATH2 = "Image"

now = time.localtime()
#curr_time = "%04d년 %02d월 %02d일 %02d시 %02d분 %02d초" % (now.tm_year, now.tm_mon, now.tm_mday, now.tm_hour, now.tm_min, now.tm_sec)

# sql = '''INSERT INTO membercar
# (CarNumber, EnteredDate, image_Data)
# VALUES
# (%s,%s,%s)
# '''

# sql = '''INSERT INTO accessrecord
#     (EnteringTime, LicenceNumber, image_data)
#     VALUES
#     (%s,%s,%s)
#     '''

def on_connect(client, userdata, flags, rc):
        print("connected!")
        client.subscribe(MQTT_PATH)
        client.subscribe(MQTT_PATH2)


def on_disconnect(client, userdata, flags, rc=0):
    print(str(rc))


def on_subscribe(client, userdata, mid, granted_qos):
    print("subscribed: " + str(mid) + " " + str(granted_qos))


def on_message(client, userdata, msg):

    conn = pymysql.connect(host='210.119.12.66', user='root', password='mysql_p@ssw0rd', db = 'final_data', charset='utf8')
    conn2 = pymysql.connect(host='210.119.12.66', user='root', password='mysql_p@ssw0rd', db = 'final_data', charset='utf8')
    #conn = pymysql.connect(host='localhost', user='root', password='mysql_p@ssw0rd', db = 'carmanagement', charset='utf8') 
    
    cursor = conn.cursor()
    cursor2 = conn2.cursor()
    
    # sql = '''INSERT INTO accessrecord
    #     (EnteringTime, LicenceNumber, image_data)
    #     VALUES
    #     (%s,%s,%s)
    #     '''
    
    sql = '''INSERT INTO membercar
        (CarNumber, EnteredDate, image_Data, registered)
        VALUES
        (%s,%s,%s,%s)
        ''' 

    # sql = '''INSERT INTO membercar
    #     (CarNumber, EnteredDate, registerd)
    #     VALUES
    #     (%s,%s,%s)
    #     ''' 
    
    sql2 = '''SELECT CarNumber FROM member'''


    engine = create_engine('mysql+pymysql://root:mysql_p@ssw0rd@210.119.12.66/final_data', echo = False)
    #engine = create_engine('mysql+pymysql://root:mysql_p@ssw0rd@210.119.12.77/carmanagement', echo = False)
    buffer = BytesIO()
    
    if msg.topic == 'common':
        arr = str(msg.payload.decode("utf-8"))
        print(arr)

        global str_data1
        str_data1 = arr[19:44]
        global str_data2
        global str_data3
        global str_registerd

        if arr[66:72] == '미인식 차량':
            str_data2 = str_data1+' '+'미인식 차량'
            str_data3 = '미인식 차량'
            #cursor.execute(sql,(arr[19:44],'미인식 차량'))
            
        else:
            str_data2 = arr[19:44]+' '+arr[66:73]
            str_data3 = arr[66:73]
            #cursor.execute(sql,(arr[19:44],arr[66:73]))
        
        cursor2.execute(sql2)
        select = list(cursor2.fetchall())
        conn2.commit()
        conn2.close()

        str_data3 = (str_data3,)
        for i in range(0,len(select)):
            if str_data3 == select[i]:
                str_registerd = '정기등록차량'
                break
            else:
                str_registerd = '일반차량'

        
    
    if msg.topic == 'Image':
        #filePath = "H:\DEV\Python\MQTT\Image"
        #filepath = "H:\DEV\StudyWPF\WPF2\FinalWpf\WpfDashBoard\bin\Debug\..\..\media/"
        filepath = "H:\DEV\StudyWPF\파이날\WPF2\FinalWpf\WpfDashBoard\media/"
        f = open(filepath+str(str_data2)+'.jpg', "wb")
        f.write(msg.payload)
        f.close()
        print('Image Received')


        im = Image.open(filepath+str(str_data2)+'.jpg','r')
   
        im.save(buffer, format='jpeg')
        img_str = base64.b64encode(buffer.getvalue())
   
        img_df = pd.DataFrame({'image_data':[img_str]})

        

        #cursor.execute(sql,(str_data1,str_data3,img_str))
        #cursor.execute(sql,(str_data3,str_data1,img_str,str_registerd)) #유진DB
        try:
            cursor.execute(sql,(str_data3,str_data1,img_str,str_registerd))
            conn.commit() 
            conn.close()
        except Exception  as e:
            print("예외발생",e)        


# 새로운 클라이언트 생성
client = mqtt.Client()
client.connect(MQTT_SERVER, 1883,60)

# 콜백 함수 설정 on_connect(브로커에 접속), on_disconnect(브로커에 접속중료), on_subscribe(topic 구독),
# on_message(발행된 메세지가 들어왔을 때)
client.on_connect = on_connect
#client.on_disconnect = on_disconnect
#client.on_subscribe = on_subscribe
client.on_message = on_message
# address : localhost, port: 1883 에 연결
# common topic 으로 메세지 발행
#client.subscribe('common', 1)
client.loop_forever()