import sys,tty,termios
import brickpi3
import time

class _Getch:
    def __call__(self):
            fd = sys.stdin.fileno()
            old_settings = termios.tcgetattr(fd)
            try:
                tty.setraw(sys.stdin.fileno())
                ch = sys.stdin.read(3)
            finally:
                termios.tcsetattr(fd, termios.TCSADRAIN, old_settings)
            return ch

def get(BP):
    inkey = _Getch()
    while(1):
            k=inkey()
            if k!='':break

    if k=='\x1b[A':
        print "up"
        BP.set_motor_power(BP.PORT_B, 100)
        BP.set_motor_power(BP.PORT_C, 100)
    elif k=='\x1b[B':
        print "down"
        BP.set_motor_power(BP.PORT_B, -100)
        BP.set_motor_power(BP.PORT_C, -100)
    elif k=='\x1b[C':
        print "right"
        BP.set_motor_power(BP.PORT_B, 0)
        BP.set_motor_power(BP.PORT_C, 100)
    elif k=='\x1b[D':
        print "left"
        BP.set_motor_power(BP.PORT_B, 100)
        BP.set_motor_power(BP.PORT_C, 0)
    else:
        print "stop"
        BP.set_motor_power(BP.PORT_B, 0)
        BP.set_motor_power(BP.PORT_C, 0)

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
        while True:
            result = get(BP)
            print(result)
            if(result==-1):
                BP.reset_all()
                break

    except (KeyboardInterrupt, SystemExit):
        print '\n...Program Stopped Manually!!!'
        BP.reset_all()
        raise

if __name__=='__main__':
        main()
