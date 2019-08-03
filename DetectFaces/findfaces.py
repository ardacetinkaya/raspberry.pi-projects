import requests
import sys
import argparse
from io import BytesIO
from PIL import Image, ImageDraw, ImageFont
import cognitive_face as CF
import time
import logging
import operator
import json

try:
    import picamera
except ImportError:
    pass

with open('config.json') as configFie:
    config = json.load(configFie)
KEY = config['key']  # Write your Cognitive Service API Key to config file
BASE_URL = 'https://detectface-faceapi.cognitiveservices.azure.com/face/v1.0'

API_URL = 'https://detectface-faceapi.cognitiveservices.azure.com/face/v1.0/detect?returnFaceAttributes=age,gender,emotion'
API_HEADER = {'Ocp-Apim-Subscription-Key': KEY,
              'Content-Type': 'application/octet-stream'}
API_PARAMS = {'language': 'unk'}
FORMAT = '%(asctime)-15s %(message)s'
logging.basicConfig(format=FORMAT)


def takePhoto():
    # Some custom path to save taken photo
    resultPath = '/home/pi/camera/detectface/image.jpg'
    try:
        with picamera.PiCamera() as camera:
            camera.resolution = (640, 480)
            camera.framerate = 90
            camera.video_stabilization = True
            camera.start_preview()
            time.sleep(1)
            camera.capture(resultPath)
            camera.stop_preview()
    except Exception, e:
        logging.error(str(e))

    return resultPath


def getFaceFromURL(img_url):
    faces = None
    try:
        CF.Key.set(KEY)
        CF.BaseUrl.set(BASE_URL)
        faces = CF.face.detect(img_url)
    except Exception, e:
        logging.error(str(e))
    return faces


def getFaceFromPath(photo):
    data = None
    try:
        img = Image.open(photo)
        binaryImg = BytesIO()
        img.save(binaryImg, format='PNG')
        binaryImg.seek(0)
        img = binaryImg.read()
        imagedata = binaryImg.getvalue()
        binaryImg.close()

        response = requests.post(API_URL,
                                 params=API_PARAMS,
                                 headers=API_HEADER,
                                 data=imagedata)

        response.raise_for_status()
        data = response.json()
    except Exception, e:
        logging.error(str(e))
    return data


def getRectangle(faceDictionary):
    rect = faceDictionary['faceRectangle']
    left = rect['left']
    top = rect['top']
    bottom = left + rect['height']
    right = top + rect['width']
    return ((left, top), (bottom, right))


def getEmotion(faceDictionary):
    emotions = faceDictionary['faceAttributes']['emotion']
    emotion = max(emotions.iteritems(), key=operator.itemgetter(1))[0]
    return emotion


def getGender(faceDictionary):
    gender = faceDictionary['faceAttributes']['gender']
    return gender


def getAge(faceDictionary):
    age = faceDictionary['faceAttributes']['age']
    return int(age)


def writeInfo(age, gender, emotion, width, draw, x, y):
    text = str(age) + ' years old ' + gender + ' mood:' + emotion
    lines = []
    # Change font for OS
    # font = ImageFont.truetype('SFNSText.ttf', 8)  
    font = ImageFont.load_default()
    if font.getsize(text)[0] <= width:
        lines.append(text)
    else:
        words = text.split(' ')
        i = 0
        while i < len(words):
            line = ''
            while i < len(words) and font.getsize(line + words[i])[0] <= width:
                line = line + words[i] + ' '
                i += 1
            if not line:
                line = words[i]
                i += 1
            lines.append(line)

    i = 0
    for line in lines:
        draw.text((x, y + i), line, (255, 255, 255), font=font)
        i += 7

    return lines


def main():
    try:
        parser = argparse.ArgumentParser()
        parser.add_argument('--url', type=str)
        parser.add_argument('--path', type=str)
        args = parser.parse_args()

        if (args.url is None) and (args.path is None):
            logging.log(logging.WARNING,
                        'FindFaces.py --path <photo path> [--url <photo url>]')

        if args.url is not None:
            IMAGE_URL = args.url
            faces = getFaceFromURL(IMAGE_URL)
            response = requests.get(IMAGE_URL)
            img = Image.open(BytesIO(response.content))
        elif args.path is not None:
            IMAGE_PATH = args.path
            faces = getFaceFromPath(IMAGE_PATH)
            img = Image.open(IMAGE_PATH)
        else:
            logging.log(
                logging.WARNING, 'No argument is given. Raspberry Pi Cam. will take for your photo.Wait a second... :)')
            IMAGE_PATH = takePhoto()
            faces = getFaceFromPath(IMAGE_PATH)
            img = Image.open(IMAGE_PATH)

        draw = ImageDraw.Draw(img)

        if faces is not None:
            if len(faces) == 0:
                logging.warning('No photo is taken...')
                sys.exit(12)
            # Change font for OS
            # font = ImageFont.truetype('SFNSText.ttf', 8)
            font = ImageFont.load_default()
            for face in faces:
                coordinate = getRectangle(face)
                c = coordinate[0]
                h = c[1] + face['faceRectangle']['height']
                w = face['faceRectangle']['width'] + 5
                draw.rectangle(coordinate, outline='red')
                emotion = getEmotion(face)
                age = getAge(face)
                gender = getGender(face)
                writeInfo(str(age), gender, emotion, w, draw, c[0], h)

            timestr = time.strftime("%Y%m%d-%H%M%S")
            img.save(timestr + '_result.jpg')
            logging.warning('It''s done.')
    except Exception, e:
        logging.error('Something is wrong!!! ):')
        logging.error('Detail:' + str(e))

if __name__ == '__main__':
    main()
