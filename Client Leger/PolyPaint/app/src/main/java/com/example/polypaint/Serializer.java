/*
* source : https://stackoverflow.com/questions/8887197/reliably-convert-any-object-to-string-and-then-back-again/8887244
* */
package com.example.polypaint;

import java.io.ByteArrayInputStream;
import java.io.ByteArrayOutputStream;
import java.io.ObjectInputStream;
import java.io.ObjectOutputStream;

public class Serializer {

    public String serialize(Object object){
        String serializedObject = "";
        try {
            ByteArrayOutputStream bo = new ByteArrayOutputStream();
            ObjectOutputStream so = new ObjectOutputStream(bo);
            so.writeObject(object);
            so.flush();
            serializedObject = bo.toString();
        } catch (Exception e) {
            System.out.println(e);
        }

        return serializedObject;
    }

    public Object deserialize(String objectString){
        Connector connector = new Connector();
        try {
            byte b[] = objectString.getBytes();
            ByteArrayInputStream bi = new ByteArrayInputStream(b);
            ObjectInputStream si = new ObjectInputStream(bi);
            connector = (Connector) si.readObject();
        } catch (Exception e) {
            System.out.println(e);
        }

        return connector;
    }
}
