# CheckDistance

CheckDistance is a simple IoT project for the people who wants to start and learn something about IoT. Like me :)

For now, the project simply checks the distance between HC-SR04 Ultrasonic sensor and any approaching object. HC-SR04 Ultrasonic sensor is connected to a Raspberry Pi Model B. To connect HC-SR04 to your Raspberry Pi, you may just dig into Google. This repository is going to be mostly about coding.

###Raspberry Pi Model B - Pin Diagram
![Pin Diagram](https://github.com/ardacetinkaya/IoT/blob/master/CheckDistance/gpiopins.png)

###Raspberry Pi Model B - Pin Connections
![Pin Connections](https://github.com/ardacetinkaya/IoT/blob/master/CheckDistance/RaspberryPi_Pins.JPG)

###Sensor Connections
![Pin Diagram](https://github.com/ardacetinkaya/IoT/blob/master/CheckDistance/Sensor.JPG)

Sensor connection;
* Ground is connected with brown cable directly to one of Pi's Ground(6th pin)
* Echo is connected with orange cable with a 1K resistor to Pi's GPIO27(13th pin)
* Trigger is connected with red cable directly to Pi's GPIO17(11th pin)
* VCC is connected with yellow cable directly to Pi's 5V(1th pin)

##Final
![Final](https://github.com/ardacetinkaya/IoT/blob/master/CheckDistance/Complete.JPG)

With the python script(*DistanceCheck.py*) in Raspberry Pi, it is checked that the distance between an object and the sensor is less then 10 cm and if it is, it is sent to the Microsoft Azure Storage account. Of course, any other cloud operations can be done with any other provider.

So, to run the script just be sure that Python is installed on your Raspberry Pi's OS. And be sure that Microsoft Azure SDK for Python is also installed. To install Microsoft Azure SDK for Python, please follow the instructions on https://pypi.python.org/pypi/azure

And a final note, I am a newbie for Python, so if there is something wrong with the scripts feel free to fix it. (:

New scripts with new integration is going to be there...

Stay tuned...

**Happy coding!!!**
