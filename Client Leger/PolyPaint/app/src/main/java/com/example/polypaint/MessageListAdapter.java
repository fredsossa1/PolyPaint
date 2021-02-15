package com.example.polypaint;

import android.content.Context;
import android.support.annotation.NonNull;
import android.support.v7.widget.RecyclerView;
import android.support.v7.widget.RecyclerView.ViewHolder;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;

import java.util.LinkedList;

public class MessageListAdapter extends RecyclerView.Adapter<ViewHolder>
{

    private static final int MESSAGE_SENT = 1;
    private static final int MESSAGE_RECEIVED = 2;
    private static final int EVENT_MESSAGE = 3;

    private LinkedList<Message> messageList;
    private Context context;

    private LayoutInflater mInflater;

    public MessageListAdapter(Context context, LinkedList<Message> messageList){
        mInflater = LayoutInflater.from(context);
        this.messageList = messageList;
        this.context = context;
    }

    @NonNull
    @Override
    public ViewHolder onCreateViewHolder(@NonNull ViewGroup viewGroup, int viewType) {

        /*View messageItemView = mInflater.inflate(R.layout.sent_message, viewGroup, false);
        return new SentMessageViewHolder(messageItemView, this);*/

        if(viewType == MESSAGE_SENT){
            View messageItemView = mInflater.inflate(R.layout.sent_message, viewGroup, false);
            return new SentMessageViewHolder(messageItemView, this);
        }
        else if(viewType == MESSAGE_RECEIVED){
            View messageItemView = mInflater.inflate(R.layout.received_message, viewGroup, false);
            return new ReceivedMessageViewHolder(messageItemView, this);
        }
        else if(viewType == EVENT_MESSAGE){
            View messageItemView = mInflater.inflate(R.layout.event_mesage, viewGroup, false);
            return new EventMessageViewHolder(messageItemView, this);
        }
        return null;
    }

    @Override
    public void onBindViewHolder(@NonNull ViewHolder viewHolder, int position) {
        Message message = messageList.get(position);

        switch (viewHolder.getItemViewType()){
            case MESSAGE_SENT:
                ((SentMessageViewHolder)viewHolder).bind(message);
                break;
            case MESSAGE_RECEIVED:
                ((ReceivedMessageViewHolder)viewHolder).bind(message);
                break;
            case EVENT_MESSAGE:
                ((EventMessageViewHolder)viewHolder).bind(message);
                break;
        }
    }

   /* @Override
    public void onBindViewHolder(@NonNull SentMessageViewHolder sentMessageViewHolder, int position) {
        String current = messageList.get(position).getMessageBody();
        sentMessageViewHolder.messageItemView.setText(current);
    }*/
   @Override
    public int getItemViewType(int position){
        Message message = messageList.get(position);
        if (message.isBelongsToCurrentUser()){
            return MESSAGE_SENT;
        }
        else{
            if(message.getType() == DataType.MSG.getValue()){
                return MESSAGE_RECEIVED;
            }
            //if(message.getType() == DataType.EVENT.getValue())
            return EVENT_MESSAGE;
        }
    }

    @Override
    public int getItemCount() {
        return messageList.size();
    }

    private class SentMessageViewHolder extends ViewHolder {

        public TextView messageItemView;
        public TextView hourItemView;
        //final MessageListAdapter mAdapter;

        public SentMessageViewHolder(View itemView, MessageListAdapter adapter) {
            super(itemView);
            messageItemView = itemView.findViewById(R.id.message_body);
            hourItemView = itemView.findViewById(R.id.sent_message_time);
            //this.mAdapter = adapter;
        }

        void bind(Message message){
            messageItemView.setText(message.getMessageBody());
            hourItemView.setText(message.getCreatedAt());
        }
    }

    private class ReceivedMessageViewHolder extends ViewHolder {

        public final TextView receivedMessageBodyItem;
        public final TextView senderName;
        public TextView hourItemView;
        //final MessageListAdapter mAdapter;

        public ReceivedMessageViewHolder (View itemView, MessageListAdapter adapter) {
            super(itemView);
            receivedMessageBodyItem = itemView.findViewById(R.id.received_message_body);
            senderName = itemView.findViewById(R.id.name);
            hourItemView = itemView.findViewById(R.id.received_message_time);
            //this.mAdapter = adapter;
        }

        void bind(Message message){
            receivedMessageBodyItem.setText(message.getMessageBody());
            senderName.setText(message.getSender().getUsername());
            hourItemView.setText(message.getCreatedAt());
        }
    }

    private class EventMessageViewHolder extends ViewHolder {

        public final TextView eventMessageBodyItem;
        public TextView hourItemView;
        //final MessageListAdapter mAdapter;

        public EventMessageViewHolder (View itemView, MessageListAdapter adapter) {
            super(itemView);
            eventMessageBodyItem = itemView.findViewById(R.id.event_message_body);
            hourItemView = itemView.findViewById(R.id.event_message_time);
            //this.mAdapter = adapter;
        }

        void bind(Message message){
            eventMessageBodyItem.setText(message.getMessageBody());
            hourItemView.setText(message.getCreatedAt());
        }
    }
}
