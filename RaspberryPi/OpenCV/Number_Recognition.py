import cv2
import numpy as np
import matplotlib.pyplot as plt
import pytesseract 
import time
import picamera
import RPi.GPIO as GPIO
import paho.mqtt.client as mqtt
import paho.mqtt.publish as publish
import json

plt.style.use('dark_background')

with picamera.PiCamera() as camera:
    camera.resolution=(960,480)
    camera.framerate = 10
    camera.start_preview()
    camera.exposure_compensation=2
    camera.exposure_mode='spotlight'
    camera.meter_mode='matrix'
    camera.image_effect='gpen'

# cap = cv2.VideoCapture(0)                                                                                                        
# if cap.isOpened == False:
#     exit()

# while True:
#     ret, img = cap.read()
#     cv2.imshow('preview',img)

#     if  cv2.waitKey(10)>=0:
#         #camera.capture('/home/pi/Python/CarNumber/captureTest.jpg')
#         cv2.imwrite('/home/pi/OpenCV/licence_plate_recognition/capturetest2.jpg',img)
#         #camera.stop_preview()
#         break

# cap.release()
# cv2.destroyAllWindows()

LED_RED = 17
LED_GREEN = 27
IR_SENSOR = 16
motor_pin =18
del_ = 0.001


GPIO.setmode(GPIO.BCM)
GPIO.setup(motor_pin, GPIO.OUT)
GPIO.setup(21,GPIO.OUT)
GPIO.setup(20,GPIO.OUT)
GPIO.setup(IR_SENSOR,GPIO.IN)
GPIO.setup(LED_GREEN, GPIO.OUT)
GPIO.setup(LED_RED, GPIO.OUT)
GPIO.output(20,False)
GPIO.output(21,True)

p= GPIO.PWM(motor_pin, 50)
p.start(0)


def Blink():
    for i in range(0,2):
        
        GPIO.output(LED_RED,False)
        GPIO.output(LED_GREEN,False)
        time.sleep(0.1)
        GPIO.output(LED_RED,True)
        GPIO.output(LED_GREEN,False)
        time.sleep(0.1)

def CaptureTest():
    cap = cv2.VideoCapture(0)                                                                                                 
    if cap.isOpened == False:
        exit()
            
    while True:
        GPIO.output(LED_RED,True)
        GPIO.output(LED_GREEN,False)

        ret, img = cap.read()
        value = GPIO.input(IR_SENSOR)

        #img = cv2.flip(img,0)
        #img = cv2.flip(img,1)
        cv2.imshow('/home/pi/OpenCV/licence_plate_recognition/preview',img)

        if  cv2.waitKey(10)>=0 or value == 0:
            Blink()
            print('Detected!\n Please wait few seconds.')
            cv2.imwrite('/home/pi/OpenCV/licence_plate_recognition/capturetest2.jpg',img)
            #camera.stop_preview()
            break
    
    cap.release()
    cv2.destroyAllWindows()


def Number_Recognition():
    img_ori = cv2.imread('/home/pi/OpenCV/licence_plate_recognition/capturetest2.jpg')

    height, width, channel = img_ori.shape  #이미지 사이즈
    
    plt.figure(figsize =(12,10))
    plt.imshow(img_ori, cmap='gray')
    plt.show()
    #plt.savefig('./fig1.png')


    gray = cv2.cvtColor(img_ori, cv2.COLOR_BGR2GRAY) #프로세싱하기 쉽게 그레이로 바꿈
    
    plt.figure(figsize=(12,10))
    plt.imshow(gray, cmap='gray')
    plt.show()


    structuringElement = cv2.getStructuringElement(cv2.MORPH_RECT, (3, 3))

    imgTopHat = cv2.morphologyEx(gray, cv2.MORPH_TOPHAT, structuringElement)
    imgBlackHat = cv2.morphologyEx(gray, cv2.MORPH_BLACKHAT, structuringElement)

    imgGrayscalePlusTopHat = cv2.add(gray, imgTopHat)
    gray = cv2.subtract(imgGrayscalePlusTopHat, imgBlackHat)

#그레이 스케일로 바꾼 후 스레쉬홀링(어댑티브): 어떤 기준점을 설정하고 그 기준점 보다 낮으면 0, 높으면 1로 처리하여 이미지를 구별하기 쉽게 만드는 것이다.(흑백으로 나누는 것)
#,노이즈를 줄이기 위한 가우시안 블러
    img_blurred = cv2.GaussianBlur(gray, ksize=(5, 5), sigmaX=0)

    img_thresh = cv2.adaptiveThreshold( 
        img_blurred, 
        maxValue=255.0, 
        adaptiveMethod=cv2.ADAPTIVE_THRESH_GAUSSIAN_C, 
        thresholdType=cv2.THRESH_BINARY_INV, 
        blockSize=19, 
        C=9
    )
    
    plt.figure(figsize=(12,10))
    plt.imshow(img_thresh, cmap='gray')
    plt.show()


#컨투어는 윤곽선, 스레쉬홀드한 이미지에서 파인드컨투어와 드로우 컨투어를 하면 그 이미지에서 윤곽선을 선으로 그려준다.
    _,contours,_ = cv2.findContours(
        img_thresh, 
        mode=cv2.RETR_LIST, 
        method=cv2.CHAIN_APPROX_SIMPLE
    )

    temp_result = np.zeros((height, width, channel), dtype=np.uint8)

    cv2.drawContours(temp_result, contours=contours, contourIdx=-1, color=(255, 255, 255))

    plt.figure(figsize=(12,10))
    plt.imshow(temp_result)
    plt.show()

    temp_result = np.zeros((height, width, channel), dtype=np.uint8)

    #contours_dict에 컨투어에 대한 정보를 저장한다.
    contours_dict = []

    #boundingRect함수를 써서 컨투어(윤곽선)을 감싸는 사각형을 구한다. (이미지에 사각형을 그리는 역할)
    for contour in contours:
        x, y, w, h = cv2.boundingRect(contour)
        cv2.rectangle(temp_result, pt1=(x, y), pt2=(x+w, y+h), color=(255, 255, 255), thickness=2)
    
    # insert to dict
        contours_dict.append({
            'contour': contour, #컨투어 저장
            'x': x,
            'y': y,
            'w': w,
            'h': h,
            'cx': x + (w / 2), #컨투어를 감싼 사각형의 중심좌표
            'cy': y + (h / 2)
        })
        
    plt.figure(figsize=(12,10))
    plt.imshow(temp_result, cmap='gray')
    plt.show()


#바운딩Rect의 최소 넓이,너비,높이,비율 (인식할 사각형의 기준을 정해준다.)
    MIN_AREA = 80
    MIN_WIDTH, MIN_HEIGHT = 2, 8
    MIN_RATIO, MAX_RATIO = 0.25, 1.0

#그중 가능한 것들을 이 배열에 다 저장한다.
    possible_contours = []

    cnt = 0
#아까 지정한 contours_dict를 돌면서 넓이와 가로대비 세로비율을 계산한다.
    for d in contours_dict:
        area = d['w'] * d['h']
        ratio = d['w'] / d['h']
    
#그 중 우리가 설정한 조건들을 만족한 것들만 번호판일 확률이 높으니 possible_contours에 append한다.
        if area > MIN_AREA \
        and d['w'] > MIN_WIDTH and d['h'] > MIN_HEIGHT \
        and MIN_RATIO < ratio < MAX_RATIO:
            d['idx'] = cnt #각 컨투어에 idx값을 매겨놓는다.
            cnt += 1
            possible_contours.append(d)
        
# visualize possible contours
    temp_result = np.zeros((height, width, channel), dtype=np.uint8)

    for d in possible_contours:
        cv2.drawContours(temp_result, d['contour'], -1, (255, 255, 255))
        cv2.rectangle(temp_result, pt1=(d['x'], d['y']), pt2=(d['x']+d['w'], d['y']+d['h']), color=(255, 255, 255), thickness=2)
        
    plt.figure(figsize=(12,10))
    plt.imshow(temp_result, cmap='gray')
    plt.show()


#위 과정까지 거치면 contourRect가 여러가지 생기는데 그 중 번호판과 관련된 것들은 일정한 간격과 크기를 가지고 연속적으로 배열되어 있을 것이다.
#그러한 특성을 설정하여 contourRect가 번호판의 Rect인지 확인 할 수 있다.

    MAX_DIAG_MULTIPLYER = 5 # 5 
    MAX_ANGLE_DIFF = 18.0 # 12.0  angle 12 ->15, area 0.5->0.3 수정 후 Andrew2 인식됨
    MAX_AREA_DIFF = 0.4 # 0.5
    MAX_WIDTH_DIFF = 0.3  #0.5
    MAX_HEIGHT_DIFF = 0.2  #0.2
    MIN_N_MATCHED = 3 # 3

#나중에 재귀함수로 사용
    def find_chars(contour_list):
        matched_result_idx = [] #최종 인덱스 값을 저장 
 
#컨투어d1과 컨투어d2를 비교하여 같으면 비교할 필요가 없으니 컨티뉴
        for d1 in contour_list:
            matched_contours_idx = []
            for d2 in contour_list:
                if d1['idx'] == d2['idx']:
                    continue

                dx = abs(d1['cx'] - d2['cx']) #'cx': x + (w / 2) 컨투어를 감싼 사각형의 중심좌표
                dy = abs(d1['cy'] - d2['cy'])
	#직각삼각형에서 c
                diagonal_length1 = np.sqrt(d1['w'] ** 2 + d1['h'] ** 2)
	#벡터a와 벡터b 사이의 거리를 구한다.
                distance = np.linalg.norm(np.array([d1['cx'], d1['cy']]) - np.array([d2['cx'], d2['cy']]))
                if dx == 0:
                    angle_diff = 90
                else:
                    angle_diff = np.degrees(np.arctan(dy / dx)) #두 컨투어간의 각도를 아크탄젠트로 구한다
                area_diff = abs(d1['w'] * d1['h'] - d2['w'] * d2['h']) / (d1['w'] * d1['h'])  #넓이 비율
                width_diff = abs(d1['w'] - d2['w']) / d1['w']  #길이 비율
                height_diff = abs(d1['h'] - d2['h']) / d1['h']  #높이 비율

            #Rectangle사이의 거리, angle , 넓이 비율, 길이,너비 비율 비교 후 기준을 만족하면 append
                if distance < diagonal_length1 * MAX_DIAG_MULTIPLYER \
                and angle_diff < MAX_ANGLE_DIFF and area_diff < MAX_AREA_DIFF \
                and width_diff < MAX_WIDTH_DIFF and height_diff < MAX_HEIGHT_DIFF:
                    matched_contours_idx.append(d2['idx'])

        # append this contour
            matched_contours_idx.append(d1['idx'])

            if len(matched_contours_idx) < MIN_N_MATCHED: #후보군의 컨투어 갯수가 3보다 작으면 번호판일 확률이 극히 낮으므로 제외 시켜버린다.
                continue

            matched_result_idx.append(matched_contours_idx) #최종 후보들을 넣어준다.

            unmatched_contour_idx = []
            for d4 in contour_list:
                if d4['idx'] not in matched_contours_idx:
                    unmatched_contour_idx.append(d4['idx'])

            unmatched_contour = np.take(possible_contours, unmatched_contour_idx)
        
        # recursive
            recursive_contour_list = find_chars(unmatched_contour)
        
            for idx in recursive_contour_list:
                matched_result_idx.append(idx) #최종 후보군이 아닌 것들도 다시한번 검사해보고 살아 남은건 추가한다.

            break

        return matched_result_idx
    
    result_idx = find_chars(possible_contours)

    matched_result = []
    for idx_list in result_idx:
        matched_result.append(np.take(possible_contours, idx_list))

# visualize possible contours
    temp_result = np.zeros((height, width, channel), dtype=np.uint8)

#result_idx를 그려보는 부분(최종 후보들을 그려보는 것)
    for r in matched_result:
        for d in r:
            cv2.drawContours(temp_result, d['contour'], -1, (255, 255, 255))
            cv2.rectangle(temp_result, pt1=(d['x'], d['y']), pt2=(d['x']+d['w'], d['y']+d['h']), color=(255, 255, 255), thickness=2)
            
    plt.figure(figsize=(12,10))
    plt.imshow(temp_result, cmap='gray')
    plt.show()


    PLATE_WIDTH_PADDING = 1.3 # 1.3
    PLATE_HEIGHT_PADDING = 1.5 # 1.5
    MIN_PLATE_RATIO = 3
    MAX_PLATE_RATIO = 10

    plate_imgs = []
    plate_infos = []

#matched_result를 돌면서 x방향?으로 순차적으로 정렬해준다.
    for i, matched_chars in enumerate(matched_result):
        sorted_chars = sorted(matched_chars, key=lambda x: x['cx'])

        plate_cx = (sorted_chars[0]['cx'] + sorted_chars[-1]['cx']) / 2   #플레이트라고 생각되는 것들의 센터x좌표 구한다.
        plate_cy = (sorted_chars[0]['cy'] + sorted_chars[-1]['cy']) / 2   #플레이트라고 생각되는 것들의 센터y좌표 구한다.
    
        plate_width = (sorted_chars[-1]['x'] + sorted_chars[-1]['w'] - sorted_chars[0]['x']) * PLATE_WIDTH_PADDING
    
        sum_height = 0
        for d in sorted_chars:
            sum_height += d['h']

        plate_height = int(sum_height / len(sorted_chars) * PLATE_HEIGHT_PADDING)  #높이 구한다.
    
#번호판이라고 생각 되어지는 컨투어들의 배열을 평평하게? 일렬로 나란히 만들기 위해서 하는 작업,
#아크사인을 이용하여 각도를 구한다.
        triangle_height = sorted_chars[-1]['cy'] - sorted_chars[0]['cy']
        triangle_hypotenus = np.linalg.norm(
            np.array([sorted_chars[0]['cx'], sorted_chars[0]['cy']]) - 
            np.array([sorted_chars[-1]['cx'], sorted_chars[-1]['cy']])
        )
    
        angle = np.degrees(np.arcsin(triangle_height / triangle_hypotenus))
    
        rotation_matrix = cv2.getRotationMatrix2D(center=(plate_cx, plate_cy), angle=angle, scale=1.0)
    #Affine을 통해서 삐뚤어진 이미지를 똑바로 돌릴 수 있다.
        img_rotated = cv2.warpAffine(img_thresh, M=rotation_matrix, dsize=(width, height))
    
   #getRectSubPix를 통해서 번호판 부분만 자른다.
        img_cropped = cv2.getRectSubPix(
            img_rotated, 
            patchSize=(int(plate_width), int(plate_height)), 
            center=(int(plate_cx), int(plate_cy))
        )
    
        if img_cropped.shape[1] / img_cropped.shape[0] < MIN_PLATE_RATIO or img_cropped.shape[1] / img_cropped.shape[0] < MIN_PLATE_RATIO > MAX_PLATE_RATIO:
            continue
    
        plate_imgs.append(img_cropped)
        plate_infos.append({
            'x': int(plate_cx - plate_width / 2),
            'y': int(plate_cy - plate_height / 2),
            'w': int(plate_width),
            'h': int(plate_height)
        })
    
        plt.subplot(len(matched_result), 1, i+1)
        plt.imshow(img_cropped, cmap='gray')
        plt.show()
    
        longest_idx = -1
        longest_text = 6

    plate_chars = []

#다시 한번 더 스레쉬 홀딩
    for i, plate_img in enumerate(plate_imgs):
        plate_img = cv2.resize(plate_img, dsize=(0, 0), fx=1.6, fy=1.6)
        _, plate_img = cv2.threshold(plate_img, thresh=0.0, maxval=255.0, type=cv2.THRESH_BINARY | cv2.THRESH_OTSU)
    
#한번 더 컨투어 구한다.
    # find contours again (same as above)
        _,contours,_ = cv2.findContours(plate_img, mode=cv2.RETR_LIST, method=cv2.CHAIN_APPROX_SIMPLE)
    
        plate_min_x, plate_min_y = plate_img.shape[1], plate_img.shape[0]
        plate_max_x, plate_max_y = 0, 0

        for contour in contours:
            x, y, w, h = cv2.boundingRect(contour)
        
            area = w * h
            ratio = w / h

            if area > MIN_AREA \
            and w > MIN_WIDTH and h > MIN_HEIGHT \
            and MIN_RATIO < ratio < MAX_RATIO:
                if x < plate_min_x:
                    plate_min_x = x
                if y < plate_min_y:
                    plate_min_y = y
                if x + w > plate_max_x:
                    plate_max_x = x + w
                if y + h > plate_max_y:
                    plate_max_y = y + h
    #번호판의 최대,최소 x,y를 구하게 되면 번호판 부분만. ..             
        img_result = plate_img[plate_min_y:plate_max_y, plate_min_x:plate_max_x]
    
#가우시안 블러,스레쉬홀드 한번 더 ,padding(여백)을 줘서 pytesseract가 인식을 잘 하도록 한다.
        img_result = cv2.GaussianBlur(img_result, ksize=(3, 3), sigmaX=0)
        _, img_result = cv2.threshold(img_result, thresh=0.0, maxval=255.0, type=cv2.THRESH_BINARY | cv2.THRESH_OTSU)
        img_result = cv2.copyMakeBorder(img_result, top=10, bottom=10, left=10, right=10, borderType=cv2.BORDER_CONSTANT, value=(0,0,0))
        global chars
        chars = pytesseract.image_to_string(img_result, lang='kor', config='--psm 7 --oem 0') #oem 0을 쓰려면 traindata를 받아야한다?
    
    # char_cnt = 0
    
    # for r in chars:
    #     if ord('가') <= ord(r) <= ord('힣'):
    #      char_cnt+= 1
    # if char_cnt > 3 or char_cnt < 1:
    #     continue
    
        result_chars = ''
        has_digit = False
        for c in chars:
            if ord('가') <= ord(c) <= ord('힣') or c.isdigit():  #숫자나 한글이 포함되어 있는지 판단하기 위함
                if c.isdigit():
                    has_digit = True
                result_chars += c
    
    #print(result_chars) 

        plate_chars.append(result_chars)
    

    char_cnt =0
    char_flag =0

    for i in range(0,len(plate_chars)):
            for r in plate_chars[i]:   
                if ord('가') <= ord(r) <= ord('힣'):
                    char_cnt+= 1

            if char_cnt == 1:
                if len(plate_chars[i]) > longest_text:
                    longest_idx = i
                    char_flag = 1
        
        
            char_cnt =0
            plt.subplot(len(plate_imgs), 1, 1)
            plt.imshow(img_result, cmap='gray')


#구한것 중에 가장 문자열이 긴 것을 우리가 찾는 번호판이라고 정한다.

    #plt.show()

#사진에서 번호판의 위치랑 예측한 결과를 찍는 부분.
    img_out = img_ori.copy()

    if char_flag == 1:
        info = plate_infos[longest_idx]
        chars = plate_chars[longest_idx]

        cv2.rectangle(img_out, pt1=(info['x'], info['y']), pt2=(info['x']+info['w'], info['y']+info['h']), color=(255,0,0), thickness=2)

        cv2.imwrite(chars + '.jpg', img_out)

        plt.figure(figsize=(12, 10))
        plt.imshow(img_out)

    elif char_flag == 0:
        chars = '미인식 차량'
        cv2.imwrite(chars + '.jpg', img_out)
    else :
        chars = '미확인 물체'
        cv2.imwrite(chars + '.jpg', img_out)
    print(chars)

    # img_out = img_ori.copy()

    # cv2.rectangle(img_out, pt1=(info['x'], info['y']), pt2=(info['x']+info['w'], info['y']+info['h']), color=(255,0,0), thickness=2)

    # cv2.imwrite(chars + '.jpg', img_out)

    plt.figure(figsize=(12, 10))
    plt.imshow(img_out)
    plt.show()

    MQTT_PATH = "common"
    MQTT_PATH2 = "Image"
    MQTT_SERVER = '210.119.12.77'

    now=time.localtime()
    curr_time = "%04d년 %02d월 %02d일 %02d시 %02d분 %02d초" % (now.tm_year, now.tm_mon, now.tm_mday, now.tm_hour, now.tm_min, now.tm_sec)

    def on_connect(client,userdata, flags, rc):
        print("Coneection OK")

    client = mqtt.Client()
    client.on_connect = on_connect
    client.connect(MQTT_SERVER,1883)
    client.loop_start()

    client.publish(MQTT_PATH,json.dumps({ 'Entering Time':curr_time,'Licence Number': chars}, ensure_ascii=False))

    client.loop_stop()

    client.disconnect()

    f=open(chars + '.jpg', "rb")
    fileContent = f.read()
    byteArr = bytearray(fileContent)

    publish.single(MQTT_PATH2, byteArr, hostname=MQTT_SERVER)

    f = open("log.txt", 'a' ,encoding="UTF8")
    f.write(" Licence Number: "+chars+"  Entering Time: "+curr_time+"\n")
    f.close()

while True:
    try:
        act = 0
        CaptureTest()
        Number_Recognition()

        if chars == '미인식 차량' or chars == '미확인 물체':
            Blink()
            act = 1

        if act == 0:
            GPIO.output(LED_RED,False)
            GPIO.output(LED_GREEN,True)

            GPIO.setup(motor_pin, GPIO.OUT)

            p.ChangeDutyCycle(7.0) 

            for i in np.arange(7.0,12.0,0.1):
                if i>=10:
                    del_ = 0.005
                p.ChangeDutyCycle(i)
                time.sleep(del_)

            time.sleep(4)

            p.ChangeDutyCycle(7.0)

            GPIO.output(LED_RED,True)
            GPIO.output(LED_GREEN,False)

            time.sleep(0.5)

            GPIO.setup(motor_pin, GPIO.IN)

            time.sleep(1.5)

        while not GPIO.input(IR_SENSOR):
            time.sleep(0.5)
            #p.stop()


    except Exception as e:
        print('오류 발생',e)
        break