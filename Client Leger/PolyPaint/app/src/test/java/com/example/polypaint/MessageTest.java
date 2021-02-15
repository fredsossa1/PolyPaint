package com.example.polypaint;

import org.junit.Test;

import static junit.framework.TestCase.assertEquals;

public class MessageTest {

    public void canCreateMessage()
    {
        Message message = new Message();
        assertEquals(message.getClass(), User.class);
    }

    @Test
    public void canCreateMessage_Using_ParametersConstructor(){
        String expectedMessageBody = "messageBody";

        User user = new User("username");
        User FakeUser = new User("usSrname");

        Message message = new Message("messageBody", user, false);
        assertEquals(message.getMessageBody(), expectedMessageBody);
        assertEquals(message.getSender(), user);

    }

}
