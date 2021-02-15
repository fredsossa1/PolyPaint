package com.example.polypaint;

import android.content.Intent;
import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.support.v7.widget.Toolbar;
import android.text.method.DigitsKeyListener;
import android.text.method.KeyListener;
import android.util.Log;
import android.view.View;
import android.widget.Button;
import android.widget.EditText;
import android.widget.Toast;

import java.net.UnknownHostException;


public class MainActivity extends AppCompatActivity //implements RoomListener
 {

     public static final String EXTRA_MESSAGE = "com.example.polypaint.extra.MESSAGE";
     protected static Connector serverConnector;
     private EditText ipAddress;
     private EditText username;
     private Button signInButton;
     private static boolean isValidIP = false;


     private static final String LOG_TAG = "TAGINGG";
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);
        Toolbar mainActivityToolbar = (Toolbar) findViewById(R.id.mainActivity_toolbar);
        setSupportActionBar(mainActivityToolbar);

        ipAddress = (EditText) findViewById(R.id.ipAddress);
        username = (EditText) findViewById(R.id.username);

        KeyListener digitsAndPeriod = DigitsKeyListener.getInstance("0123456789.");
        ipAddress.setKeyListener(digitsAndPeriod);

        signInButton = (Button) findViewById(R.id.sign_in_button);
        signInButton.setEnabled(false);

        ipAddress.addTextChangedListener(new GenericTextWatcher(signInButton));

    }


    public void login(final View view) {
        //Log.d(LOG_TAG, "Button clicked");

        serverConnector = new Connector();
        new Thread(new Runnable() {
            @Override
            public void run() {
                // TODO Replace "10.200.19.38"by ipAddress.getText().toString()
                // TODO Control Sign in button Activation or display message to inform that a field is Empty
                // TODO Display message to other users on Login (user has joined)
                // TODO Display message to other users on Logout (user has left)
                // TODO Display ... to inform that someone is typing (if you got time :-) )

                    if(serverConnector.validateIPAddress(ipAddress.getText().toString())){
                        isValidIP = true;
                        serverConnector.connect(ipAddress.getText().toString());
                        serverConnector.authenticate(DataType.AUTH, username.getText().toString());
                        Intent chatIntent = new Intent(view.getContext(), ChatActivity.class);
                        chatIntent.putExtra(EXTRA_MESSAGE, username.getText().toString());
                        startActivity(chatIntent);
                        Intent socketIntent = new Intent(view.getContext(), SocketService.class);
                        startService(socketIntent);
                    }
            }
        }).start();

        /*if(!isValidIP){
            Toast.makeText(MainActivity.this, "IP Address is incorrect or in wrong format! Please try again",Toast.LENGTH_LONG).show();
        }*/
        //username.getText().clear();
    }
}
