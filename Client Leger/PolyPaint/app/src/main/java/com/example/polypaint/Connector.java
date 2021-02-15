package com.example.polypaint;

import android.util.Log;


import com.google.gson.Gson;

import java.io.IOException;
import java.io.InputStream;
import java.io.PrintWriter;
import java.net.InetAddress;
import java.net.Socket;
import java.net.UnknownHostException;

public class Connector {


    class Authentification {
        public int getType() {
            return type;
        }

        public void setType(int type) {
            this.type = type;
        }

        public String getId() {
            return id;
        }

        public void setId(String id) {
            this.id = id;
        }

        public int type;
        public String id;
    }

    class MessageToServer {
        public int type;
        public String sender;
        public String content;
        public String timeStamp;
        public String context;
        public String name;
        public String source;
    }

    private Socket socket;
    private PrintWriter printWriter;
    private static final int SERVER_PORT = 5000;

    private static final String LOG_TAG = "ERROR";

    public void connect(String ipAddress) {

        try {
            socket = new Socket(ipAddress, SERVER_PORT);
        } catch (IOException e) {
        }
    }

    public void authenticate(DataType type, String username){
        Gson serializer = new Gson();
        try {
            printWriter = new PrintWriter(socket.getOutputStream(), true);
        } catch (IOException e) {
            e.printStackTrace();
        }
        Authentification auth = new Authentification();
        auth.type = DataType.AUTH.getValue();
        auth.id = username;
        String authBuffer = serializer.toJson(auth);
        Log.i("JSON ", authBuffer);

        if (printWriter != null && !printWriter.checkError()) {
            printWriter.println(authBuffer);
            printWriter.flush();
        }
    }

    public void sendMessage(DataType type, String username, String message, String messageTime) {

            Gson serializer = new Gson();
            switch (type.getValue()) {
                case 1:
                    try {
                        printWriter = new PrintWriter(socket.getOutputStream(), true);
                    } catch (IOException e) {
                        e.printStackTrace();
                    }
                    MessageToServer messageToSend = new MessageToServer();
                    messageToSend.type= DataType.MSG.getValue();
                    messageToSend.sender = username;
                    messageToSend.content = message;
                    messageToSend.timeStamp = messageTime;

                    String msgBuffer = serializer.toJson(messageToSend);
                    Log.i("JSON ", msgBuffer);

                    if (printWriter != null && !printWriter.checkError()) {
                        printWriter.println(msgBuffer);
                        printWriter.flush();

                    }
                    break;

                default:
                    break;
            }
        }


    public MessageToServer readMessage() throws IOException {
        Gson serializer = new Gson();
        byte [] data;
        data = new byte[256];

        InputStream is = socket.getInputStream();
        is.read(data,0,data.length);
        MessageToServer packet;

        String message = new String(data,"UTF-8");
        int indexOfCut = message.indexOf("}");
        message = message.substring(0, indexOfCut+1);
        System.out.println("SERVER: " + message);
        packet = serializer.fromJson(message, MessageToServer.class);
        System.out.println("SERVER: " + packet.content);

        return packet;
        /*InputStream is = socket.getInputStream();
        BufferedReader br = new BufferedReader(new InputStreamReader(is));
        String line ;
        String message = "";
        while ((line = br.readLine()) != null) {
            if (line.isEmpty()) {
                break;
            }
            message += line;
        }
        MessageToServer packet;
        packet = serializer.fromJson(message, MessageToServer.class);
        return packet;*/
    }


    public void disconnect(){
        printWriter.close();
        try {
            socket.close();
        } catch (IOException e) {
            e.printStackTrace();
        }
    }


    public  boolean isConnected(){
        if((socket != null)&& (socket.isConnected()))
            return true;
        else
            return false;
    }

    public boolean validateIPAddress(String ipAdress) {

        boolean isValid = false;
        String temp = ipAdress;
        int periodOccurence = temp.length() - temp.replace(".","").length();
        System.out.println("OCCURENCE : " + periodOccurence);

        if (periodOccurence != 3)
            isValid = false;
        else {
            try {
                InetAddress.getByName(ipAdress);
                isValid = true;
            } catch (UnknownHostException e) {
                isValid = false;
                e.printStackTrace();
            }

        }

        return  isValid;
    }

}
