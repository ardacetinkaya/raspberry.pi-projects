#!/usr/bin/python

import time
import uuid
import RPi.GPIO as GPIO
from azure.storage.table import TableService, Entity

#This class helps us to store distance data in Azure.
class MonitorManager(object):
    def __init__(self):
		#Please first create Azure Storage account and obtain your account name and key
		self._tableService = TableService(account_name='YOUR_ACCOUNT_NAME', account_key='YOUR_ACCOUNT_KEY')
		self._tableService.create_table('sensordata')

    def Insert(self, distance, currentTime):
		distanceData = Entity()
		distanceData.PartitionKey = 'sensorKey'
		distanceData.RowKey = str(uuid.uuid1())
		distanceData.distance = str(distance)
		distanceData.time = str(currentTime)
		self._tableService.insert_entity('sensordata', distanceData)

#Distance checker
class DistanceSensor(object):
    def Init(self):
        print("Sensors are initializing...")
        GPIO.setwarnings(False)
        GPIO.setmode(GPIO.BCM)

        GPIO.setup(17, GPIO.OUT)
        GPIO.setup(27, GPIO.IN)
        GPIO.output(17, GPIO.LOW)
        # Wait for some time to settle
        time.sleep(0.3)

    def CheckDistance(self):

        GPIO.output(17, True)
        time.sleep(0.1)
        GPIO.output(17, False)

        while GPIO.input(27) == 0:
            signalOff = time.time()

        while GPIO.input(27) == 1:
            signalOn = time.time()

        timePassed = signalOn - signalOff
        distance = timePassed * 17000
        print("Distance: %.2f cm" %distance)
        return distance

    def Dispose(self):
        GPIO.output(17, False)
        GPIO.cleanup()

#Startting point of the process, you may think as Main()
monitor = MonitorManager()

sensor = DistanceSensor()
sensor.Init()

try:
    print ("Distance Check is started... " +
           time.strftime('%Y-%m-%d %H:%M:%S', time.localtime()))
    while True:
        distance = sensor.CheckDistance()
        if distance < 10:
            monitor.Insert(distance, time.strftime(
                '%Y-%m-%d %H:%M:%S', time.localtime()))
        time.sleep(0.0001)
except KeyboardInterrupt:
    print("Exit")
except Exception as e:
    print("An error is occured. Detail: " + e.message)
finally:
    sensor.Dispose()
#end of Main()
