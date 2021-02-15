package com.example.polypaint;

import android.content.Intent;
import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.support.v7.widget.LinearLayoutManager;
import android.support.v7.widget.RecyclerView;
import android.support.v7.widget.Toolbar;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;
import android.widget.EditText;
import android.widget.ImageButton;

import java.util.LinkedList;

import static com.example.polypaint.MainActivity.EXTRA_MESSAGE;

public class ChatActivity extends AppCompatActivity {

    protected static LinkedList<Message> messageList = new LinkedList<>();
    private User currentUser;
    protected static RecyclerView recyclerView;
    protected static MessageListAdapter messageListAdapter;
    private String username;
    private Connector serverConnector;
    protected static int messageListSize;


    private static final String LOG_TAG = "CHECKERROR";

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_chat);
        Toolbar chatActivityToolbar = (Toolbar) findViewById(R.id.chatActivity_toolbar);
        setSupportActionBar(chatActivityToolbar);

        Intent intent = getIntent();


        recyclerView = findViewById(R.id.recyclerview);
        messageListAdapter = new MessageListAdapter(this, messageList);
        recyclerView.setAdapter(messageListAdapter);
        recyclerView.setLayoutManager(new LinearLayoutManager(this));
        username = intent.getStringExtra(EXTRA_MESSAGE);
        currentUser = new User(username);
        serverConnector = MainActivity.serverConnector;
    }

    public void sendMessage(View view) {

        messageListSize = messageList.size();
        ImageButton button = (ImageButton) findViewById(R.id.send_Button);
        EditText mEdit = (EditText) findViewById(R.id.messageText);
        if (!mEdit.getText().toString().isEmpty()) {
            if (mEdit.getText().toString().trim().length() > 0) {
                button.setEnabled(true);
                final Message message = new Message(mEdit.getText().toString(), currentUser, true, DataType.MSG.getValue());

                new Thread(new Runnable() {
                    @Override
                    public void run() {
                        serverConnector.sendMessage(DataType.MSG, username, message.getMessageBody(), message.getCreatedAt());
                    }
                }).start();

                messageList.addLast(message);
                recyclerView.getAdapter().notifyItemInserted(messageListSize);
                recyclerView.smoothScrollToPosition(messageListSize);

                mEdit.getText().clear();

            }
        }
    }

    /**
     * @source Based on https://developer.android.com/guide/topics/ui/menus 
     */
    @Override
    public boolean onCreateOptionsMenu(Menu menu) {
        // Inflate the menu_chatactivity; this adds items to the action bar if it is present.
        getMenuInflater().inflate(R.menu.menu_chatactivity, menu);
        return true;
    }

    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        // Handle action bar item clicks here. The action bar will
        // automatically handle clicks on the Home/Up button, so long
        // as you specify a parent activity in AndroidManifest.xml.
        int id = item.getItemId();

        //noinspection SimplifiableIfStatement
        if (id == R.id.action_settings) {
            serverConnector.disconnect();
            // Empty messages stack and finishing the activity
            messageList.clear();;
            this.finish();

        }

        return super.onOptionsItemSelected(item);
    }
}
