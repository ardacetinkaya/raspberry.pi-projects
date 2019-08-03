#!/usr/bin/env python
# -*- coding: utf-8 -*-
import time
import datetime
import subprocess
import os.path
import RPi.GPIO as GPIO

from oled_device import get_device
from luma.core.render import canvas

from PIL import Image
from PIL import ImageDraw
from PIL import ImageFont

font = ImageFont.load_default()

KEY_UP_PIN = 6
KEY_DOWN_PIN = 19
KEY_LEFT_PIN = 5
KEY_RIGHT_PIN = 26
KEY_PRESS_PIN = 13
KEY1_PIN = 21
KEY2_PIN = 20
KEY3_PIN = 16

GPIO.setmode(GPIO.BCM)
GPIO.setup(KEY_UP_PIN, GPIO.IN, pull_up_down=GPIO.PUD_UP)
GPIO.setup(KEY_DOWN_PIN, GPIO.IN, pull_up_down=GPIO.PUD_UP)
GPIO.setup(KEY_LEFT_PIN, GPIO.IN, pull_up_down=GPIO.PUD_UP)
GPIO.setup(KEY_RIGHT_PIN, GPIO.IN, pull_up_down=GPIO.PUD_UP)
GPIO.setup(KEY_PRESS_PIN, GPIO.IN, pull_up_down=GPIO.PUD_UP)
GPIO.setup(KEY1_PIN, GPIO.IN, pull_up_down=GPIO.PUD_UP)
GPIO.setup(KEY2_PIN, GPIO.IN, pull_up_down=GPIO.PUD_UP)
GPIO.setup(KEY3_PIN, GPIO.IN, pull_up_down=GPIO.PUD_UP)


def run(device, draw):

    padding = 2
    shape_width = 20
    top = padding
    bottom = device.height - padding - 1
    x = padding + 10
    # 12,2,32,61
    # 34,42,54,61
    # 56,42,54,61
    # draw.rectangle((x, top, x + shape_width, bottom), outline="blue", fill=1)
    # x += shape_width + padding

    draw.rectangle((100, top, 100 + shape_width, bottom - 40), outline="blue", fill=0)
    x += shape_width + padding

    draw.rectangle((100, top + 20, 100 + shape_width,bottom - 20), outline="blue", fill=0)
    x += shape_width + padding

    draw.rectangle((100, top + 40, 100 + shape_width, bottom), outline="blue", fill=0)

    draw.text((2 * padding, top + 4), "Hello World!", fill=1)

    ipcmd = "hostname -I | cut -d\' \' -f1"
    IP = subprocess.check_output(ipcmd, shell=True)
    draw.text((padding, top + 16),  "IP:" + str(IP),  font=font, fill=1)

    now = datetime.datetime.now()
    draw.text((padding, bottom - 20), now.strftime("%d.%m.%Y"), fill=1)
    draw.text((padding, bottom - 10), now.strftime("%H:%M:%S"), fill=1)
    time.sleep(0.1)

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

    draw.text((2, 2 + 8),     str(CPU), font=font, fill=255)
    draw.text((2, 2 + 16),    str(MemUsage),  font=font, fill=255)
    draw.text((2, 2 + 25),    str(Disk),  font=font, fill=255)


def main():
    device = get_device()
    try:
        while True:
            with canvas(device) as draw:
                if GPIO.input(KEY1_PIN) == False:
                    device.clear()
                elif GPIO.input(KEY2_PIN) == False:
                    printlogo(device)
                elif GPIO.input(KEY3_PIN) == False:
                    printinfo(draw)
                else:
                    run(device, draw)

    except Exception, e:
        print("ERROR: " + str(e))

if __name__ == "__main__":
    try:
        main()
    except KeyboardInterrupt:
        pass
