import sys,tty
import brickpi3
import time

def get(BP):
    inkey = sys.argv[1]

    if inkey=='up':
        print "up"
        BP.set_motor_power(BP.PORT_B, 100)
        BP.set_motor_power(BP.PORT_C, 100)
    elif inkey=='down':
        print "down"
        BP.set_motor_power(BP.PORT_B, -100)
        BP.set_motor_power(BP.PORT_C, -100)
    elif inkey=='right':
        print "right"
        BP.set_motor_power(BP.PORT_B, 0)
        BP.set_motor_power(BP.PORT_C, 100)
    elif inkey=='left':
        print "left"
        BP.set_motor_power(BP.PORT_B, 100)
        BP.set_motor_power(BP.PORT_C, 0)
    elif inkey=='optionright':
        BP.set_motor_power(BP.PORT_B, 0)
        BP.set_motor_power(BP.PORT_C, 0)
        BP.set_motor_power(BP.PORT_A, 0)
        sys.exit(0)
    elif inkey=='x':
        BP.set_motor_power(BP.PORT_A, 5)
    elif inkey=='z':
        BP.set_motor_power(BP.PORT_A,-5)
    elif inkey=='c':
        BP.set_motor_power(BP.PORT_A,0)
    else:
        print "stop"
        BP.set_motor_power(BP.PORT_B, 0)
        BP.set_motor_power(BP.PORT_C, 0)
        BP.set_motor_power(BP.PORT_A, 0)

    time.sleep(0.02)

def main():
    BP = brickpi3.BrickPi3()
    try:
        BP.offset_motor_encoder(BP.PORT_A, BP.get_motor_encoder(BP.PORT_A))
        BP.offset_motor_encoder(BP.PORT_B, BP.get_motor_encoder(BP.PORT_B))
        BP.offset_motor_encoder(BP.PORT_C, BP.get_motor_encoder(BP.PORT_C))
        BP.offset_motor_encoder(BP.PORT_D, BP.get_motor_encoder(BP.PORT_D))
    except IOError as error:
        print(error)

    try:
        result = get(BP)

    except (KeyboardInterrupt, SystemExit):
        print '\nMotors are stopped'
        BP.reset_all()
        raise

if __name__=='__main__':
        main()
