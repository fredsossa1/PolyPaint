package com.example.polypaint;

public enum DataType {
    AUTH(0),
    MSG(1),
    ERROR(2),
    EVENT(3);

    public int getValue() {
        return value;
    }

    public void setValue(int value) {
        this.value = value;
    }

    private int value;

    private DataType(int value){
        this.value = value;
    }
}

