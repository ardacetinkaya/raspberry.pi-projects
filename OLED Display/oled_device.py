# -*- coding: utf-8 -*-

import sys
import logging

from luma.core import error
from luma.core.interface.serial import i2c, spi
from luma.oled.device import sh1106
import RPi.GPIO as GPIO

# logging
logging.basicConfig(
    level=logging.DEBUG,
    format='%(asctime)-15s - %(message)s'
)
# ignore PIL debug messages
logging.getLogger('PIL').setLevel(logging.ERROR)

def get_device():
    # create device
    try:
        USER_I2C = 0

        if  USER_I2C == 1:
            GPIO.setmode(GPIO.BCM)
            GPIO.setup(RST,GPIO.OUT)	
            GPIO.output(RST,GPIO.HIGH)
            
            serial = i2c(port=1, address=0x3c)
        else:
            serial = spi(device=0, port=0, bus_speed_hz = 8000000, transfer_size = 4096, gpio_DC = 24, gpio_RST = 25)

        device = sh1106(serial, rotate=2)
    except error.Error as e:
        parser.error(e)

    return device