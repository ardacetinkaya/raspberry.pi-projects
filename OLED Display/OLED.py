#!/usr/bin/env python
# -*- coding: utf-8 -*-
import os
import sys
import time
import datetime
import subprocess
import os.path
import RPi.GPIO as GPIO
import thread
import json

from oled_device import get_device
from flask import Flask, request,Response,make_response,jsonify
from luma.core.render import canvas
from luma.core.legacy import show_message
from luma.core.sprite_system import framerate_regulator

from PIL import Image
from PIL import ImageDraw
from PIL import ImageFont

app = Flask(__name__)
KEY_UP_PIN = 6
KEY_DOWN_PIN = 19
KEY_LEFT_PIN = 5
KEY_RIGHT_PIN = 26
KEY_PRESS_PIN = 13
KEY1_PIN = 21
KEY2_PIN = 20
KEY3_PIN = 16

message="Hello World!"
action="clear"
ison=0

def isOpened(value):
    global ison
    if value ==0:
        ison=1
        print("on")
    else:
        ison=0
        print("off")

GPIO.setmode(GPIO.BCM)
GPIO.setup(KEY_UP_PIN, GPIO.IN, pull_up_down=GPIO.PUD_UP)
GPIO.setup(KEY_DOWN_PIN, GPIO.IN, pull_up_down=GPIO.PUD_UP)
GPIO.setup(KEY_LEFT_PIN, GPIO.IN, pull_up_down=GPIO.PUD_UP)
GPIO.setup(KEY_RIGHT_PIN, GPIO.IN, pull_up_down=GPIO.PUD_UP)
GPIO.setup(KEY_PRESS_PIN, GPIO.IN, pull_up_down=GPIO.PUD_UP)
GPIO.setup(KEY1_PIN, GPIO.IN, pull_up_down=GPIO.PUD_UP)
GPIO.setup(KEY2_PIN, GPIO.IN, pull_up_down=GPIO.PUD_UP)
GPIO.setup(KEY3_PIN, GPIO.IN, pull_up_down=GPIO.PUD_UP)

GPIO.add_event_detect(KEY_PRESS_PIN,GPIO.FALLING,callback=lambda x: isOpened(ison))

def createfont(name, size):
    if not name.strip():
        return ImageFont.load_default()

    font_path = os.path.abspath(os.path.join(os.path.dirname(__file__), 'fonts', name))
    return ImageFont.truetype(font_path, size)

def run(device, draw):

    padding = 2
    shape_width = 20
    top = padding
    bottom = device.height - padding - 1
    x = padding + 10

    ipcmd = "hostname -I | cut -d\' \' -f1"
    cpucmd = "top -bn1 | grep load | awk '{printf \"CPU Load: %.2f\", $(NF-2)}'"
    memcmd = "free -m | awk 'NR==2{printf \"Mem: %s/%sMB %.2f%%\", $3,$2,$3*100/$2 }'"
    diskcmd = "df -h | awk '$NF==\"/\"{printf \"Disk: %d/%dGB %s\", $3,$2,$5}'"

    IP = subprocess.check_output(ipcmd, shell=True)
    CPU = subprocess.check_output(cpucmd, shell=True)
    MemUsage = subprocess.check_output(memcmd, shell=True)
    Disk = subprocess.check_output(diskcmd, shell=True)
    
    draw.text((padding, bottom-60),  str(CPU), fill=1)
    draw.text((padding, bottom-50),  str(MemUsage), fill=1)
    draw.text((padding, bottom-40),  str(Disk), fill=1)
    draw.text((padding, bottom-30),  "IP:" + str(IP), fill=1)

    now = datetime.datetime.now()
    draw.text((padding, bottom - 20), now.strftime("%d.%m.%Y"), fill=1)
    draw.text((padding, bottom - 10), now.strftime("%H:%M:%S"), fill=1)
    time.sleep(0.1)

    draw.rectangle(device.bounding_box, outline="blue")

def empty(device, draw):

    padding = 2
    shape_width = 20
    top = padding
    bottom = device.height - padding - 1

    if str(action)=="print":
        draw.text((2 * padding, top), str(message),font=font,fill=1)      

    draw.rectangle(device.bounding_box, outline="blue")

def printlogo(device):
    img_path = os.path.abspath(os.path.join(os.path.dirname(__file__),'images', 'lego.jpg'))
    logo = Image.open(img_path).convert("RGBA")
    fff = Image.new(logo.mode, logo.size, (255,) * 4)

    background = Image.new("RGBA", device.size, "white")
    posn = ((device.width - logo.width) // 2, 0)

    for angle in range(0, 360, 2):
        rot = logo.rotate(angle, resample=Image.BILINEAR)
        img = Image.composite(rot, fff, rot)
        background.paste(img, posn)
        device.display(background.convert(device.mode))

    time.sleep(2)

def printinfo(draw):
    cpucmd = "top -bn1 | grep load | awk '{printf \"CPU Load: %.2f\", $(NF-2)}'"
    memcmd = "free -m | awk 'NR==2{printf \"Mem: %s/%sMB %.2f%%\", $3,$2,$3*100/$2 }'"
    diskcmd = "df -h | awk '$NF==\"/\"{printf \"Disk: %d/%dGB %s\", $3,$2,$5}'"

    CPU = subprocess.check_output(cpucmd, shell=True)
    MemUsage = subprocess.check_output(memcmd, shell=True)
    Disk = subprocess.check_output(diskcmd, shell=True)

    draw.text((2, 2 + 8),     str(CPU), fill=1)
    draw.text((2, 2 + 16),    str(MemUsage), fill=1)
    draw.text((2, 2 + 25),    str(Disk), fill=1)

@app.route('/',methods=['POST'])
def receivemessage():
    global message
    global action
    content = {"message": "OK"}
    try:
        data = json.loads(request.data)
        if len(data["Message"]) > 15:
            content = {"message":"Too long messasge"}
            return make_response(jsonify(content), 400)
        message = data["Message"]
        action = data["Action"]
    except Exception, e:
        print("ERROR: "+ str(e))
        content = {"message": "ERROR"+str(e)}
        return make_response(jsonify(content), 501)
    
    return make_response(jsonify(content), 200)

def server():
    global message
    app.run(host='0.0.0.0')

font = createfont('',14)

def main():
    global ison
    device = get_device()
    try:
        while True:
            with canvas(device) as draw:
                if ison==1:
                    run(device, draw)
                elif GPIO.input(KEY1_PIN) == False:
                    empty(device,draw)
                elif GPIO.input(KEY2_PIN) == False:
                    printlogo(device)
                elif GPIO.input(KEY3_PIN) == False:
                    printinfo(draw)
                elif str(action)=="print":
                    run(device,draw)
                else:
                    if str(action)=="clear":
                        device.clear()

    except Exception, e:
        print("ERROR: " + str(e))

if __name__ == "__main__":
    try:
        thread.start_new_thread(server,())
        main()
    except KeyboardInterrupt:
        pass

