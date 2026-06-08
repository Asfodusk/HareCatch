using UnityEngine;
using UnityEngine.UI;

public class MusicSetting : MonoBehaviour
{
    public Button[] buttons; // все кнопки, для переключения прозрачности

    public void Setting(int buttonNumber) //принимает номер нажатой кнопки 
    {
        // текущая кнопка
        Image buttonImage = buttons[buttonNumber-1].GetComponentInChildren<Image>(); // изображение
        Color alpha = buttonImage.color; // цвет изображения

        if (alpha.a == 0f) // выбранная кнопка прозрачная
        {
            //делаем непрозрачной
            alpha.a = 1f;
            buttonImage.color = alpha; 
                
            //показываем все до нее
            for (int i = 0; i < buttonNumber-1; i++)
            {
                Image otherBtnImage = buttons[i].GetComponentInChildren<Image>(); //изображение
                Color otherAlpha = otherBtnImage.color; //цвета
                otherAlpha.a = 1f; // установить непрозрачность на 100
                otherBtnImage.color = otherAlpha; //присвоение новой прозрачности
            }
        }
        else // выбранная кнопка непрозрачная
        {
            //делаем прозрачной
                alpha.a = 0f;
                buttonImage.color = alpha; 
            
            //скрываем все после нее
            for (int i = buttonNumber; i < buttons.Length; i++)
            {
                Image otherBtnImage = buttons[i].GetComponentInChildren<Image>(); //изображение
                Color otherAlpha = otherBtnImage.color; //цвета
                otherAlpha.a = 0f; // установить непрозрачность на 0
                otherBtnImage.color = otherAlpha; //присвоение новой прозрачности
            }
        }
        
    }
}