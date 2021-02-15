package com.example.polypaint;

import java.text.DateFormat;
import java.text.SimpleDateFormat;
import java.util.Calendar;

public class Message {

    private String message;
    private User sender;
    private String createdAt;
    private boolean belongsToCurrentUser;

    public int getType() {
        return type;
    }

    private int type;

    public Message(){};

    public Message(String message, User sender, boolean belongsToCurrentUser, int type){
        this.message = message;
        this.sender = sender;
        this.belongsToCurrentUser = belongsToCurrentUser;
        this.type = type;

        DateFormat dateFormat = new SimpleDateFormat("HH:mm:ss");
        createdAt = dateFormat.format(Calendar.getInstance().getTime());
    }

    public User getSender() {
        return sender;
    }

    public void setSender(User sender) {
        this.sender = sender;
    }

    public String getMessageBody() {
        return message;
    }

    public void setMessage(String message) {
        this.message = message;
    }

    public boolean isBelongsToCurrentUser() {
        return belongsToCurrentUser;
    }

    public String getCreatedAt() {
        return createdAt;
    }
}
