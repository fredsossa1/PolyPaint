package com.example.polypaint;

import android.app.IntentService;
import android.content.Intent;
import android.os.Handler;

import java.io.IOException;


/**
 * An {@link IntentService} subclass for handling asynchronous task requests in
 * a service on a separate handler thread.
 * <p>
 * TODO: Customize class - update intent actions and extra parameters.
 */
public class SocketService extends IntentService {

    public SocketService() {

        super("SocketService");
    }

    @Override
    protected void onHandleIntent(Intent intent) {
        if (intent != null) {
            while (MainActivity.serverConnector.isConnected()) {
                Connector.MessageToServer messageToServer = null;
                try {
                    messageToServer = MainActivity.serverConnector.readMessage();

                    if (messageToServer.type == DataType.MSG.getValue()) {
                        final Message receivedMessage = new Message(messageToServer.content, new User(messageToServer.sender), false, DataType.MSG.getValue());

                        runOnUiThread(new Runnable() {
                            @Override
                            public void run() {
                                ChatActivity.messageList.addLast(receivedMessage);
                                ChatActivity.messageListAdapter.notifyDataSetChanged();
                                ChatActivity.recyclerView.smoothScrollToPosition(ChatActivity.messageListAdapter.getItemCount());
                            }
                        });
                        ;
                    } else if (messageToServer.type == DataType.EVENT.getValue()) {
                        if (messageToServer.name.equals("clientDisconnection")) {
                            final Message receivedMessage = new Message(messageToServer.source + " has left the chat", new User(messageToServer.source), false, DataType.EVENT.getValue());

                            runOnUiThread(new Runnable() {
                                @Override
                                public void run() {
                                    ChatActivity.messageList.addLast(receivedMessage);
                                    ChatActivity.messageListAdapter.notifyDataSetChanged();
                                    ChatActivity.recyclerView.smoothScrollToPosition(ChatActivity.messageListAdapter.getItemCount());
                                }
                            });
                            ;

                        }else if(messageToServer.name.equals("clientConnection")){
                            final Message receivedMessage = new Message(messageToServer.source + " has joined the chat", new User(messageToServer.source), false, DataType.EVENT.getValue());

                            runOnUiThread(new Runnable() {
                                @Override
                                public void run() {
                                    ChatActivity.messageList.addLast(receivedMessage);
                                    ChatActivity.messageListAdapter.notifyDataSetChanged();
                                    ChatActivity.recyclerView.smoothScrollToPosition(ChatActivity.messageListAdapter.getItemCount());
                                }
                            });
                            ;
                        }
                    }

                } catch (IOException e) {
                    e.printStackTrace();

                }
            }

        }
    }

    Handler handler;

    @Override
    public void onCreate() {
        // Handler will get associated with the current thread,
        // which is the main thread.
        handler = new Handler();
        super.onCreate();
    }

    private void runOnUiThread(Runnable runnable) {
        handler.post(runnable);
    }

}
