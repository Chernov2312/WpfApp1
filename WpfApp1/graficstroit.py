from math import *


def function(stroke: str, x: int):
    return eval(stroke.split("=")[-1].replace("x", str(x)))