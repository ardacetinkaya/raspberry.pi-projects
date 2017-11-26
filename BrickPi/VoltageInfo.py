#!/usr/bin/env python


from __future__ import print_function 
from __future__ import division                                  ''

import sqlite3
import time     
import brickpi3 

BP = brickpi3.BrickPi3() 

try:
    conn = sqlite3.connect('../Database/pidata.db')
    c = conn.cursor()
    print("Welcome")
    while True:
        voltage9 = BP.get_voltage_9v()
        voltage3 = BP.get_voltage_3v3()
        voltage5 = BP.get_voltage_5v()
        params = [(voltage9,voltage3,voltage5)]
        
        c.executemany("INSERT INTO PiData (Voltage9, Voltage3,Voldate5, currentdate, currentime, device) VALUES(?,?,?,date('now'),time('now'),'Miyop')",params)
        conn.commit()
        time.sleep(60) #log per minute

except KeyboardInterrupt:
    BP.reset_all()
    conn.close()