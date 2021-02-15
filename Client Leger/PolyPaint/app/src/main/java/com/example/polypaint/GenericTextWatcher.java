/**
 * source : https://stackoverflow.com/questions/5702771/how-to-use-single-textwatcher-for-multiple-edittexts
 */
package com.example.polypaint;

import android.text.Editable;
import android.text.TextWatcher;
import android.view.View;


public class GenericTextWatcher implements TextWatcher {

    private View view;
    public GenericTextWatcher(View view) {
        this.view = view;
    }

    public void beforeTextChanged(CharSequence sequence, int i, int i1, int i2) {}
    public void onTextChanged(CharSequence sequence, int i, int i1, int i2) {
        if(sequence.toString().trim().length()==0){
            view.setEnabled(false);
        } else {
            view.setEnabled(true);
        }
    }

    public void afterTextChanged(Editable editable) {
    }
}
