from math import *


def function(stroke: str, x: float):
    return eval(stroke.split("=")[-1].replace("x", str(x))) if -25 <= eval(stroke.split("=")[-1].replace("x", str(x))) <= 25 else "root"