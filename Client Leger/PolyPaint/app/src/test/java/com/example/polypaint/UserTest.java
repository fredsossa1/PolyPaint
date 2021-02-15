package com.example.polypaint;
import org.junit.Test;

import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertNotNull;

public class UserTest{

    @Test
    public void canCreateUser_Using_EmptyConstructor()
    {
        User user = new User("");
        assertEquals(user.getClass(), User.class);
    }

    @Test
    public void canCreateUser_Using_ParametersConstructor(){
        String expectedUsername = "username";
        User user = new User("username");
        assertEquals(user.getUsername(), expectedUsername);
    }

    @Test
    public void Can_Set_Username(){
        String expectedUsername = "username";
        User user = new User("");
        user.setUsername("username");
        assertEquals(user.getUsername(), expectedUsername);
    }
}
