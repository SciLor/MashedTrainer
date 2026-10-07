CC ?= gcc
CFLAGS ?= -O2 -Wall

all: mfl_dummy

mfl_dummy: mfl_dummy.c
	$(CC) $(CFLAGS) $< -o $@

clean:
	rm -f mfl_dummy mfl_dummy.exe

.PHONY: all clean

