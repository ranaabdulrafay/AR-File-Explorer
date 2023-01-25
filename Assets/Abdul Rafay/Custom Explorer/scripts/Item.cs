using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace AR.Explorer
{
    public class Item : MonoBehaviour
    {
        public Image Icon;
        public Image BackGround;
        public Text Name;
        public Button OnClickBtn;

        public string Path;
        private bool isSelected;
        Color SelectedColor = new Color(0.41f, 0.73f, 1);
        public bool IsSelected
        {
            get
            {
                return isSelected;
            }
            set
            {
                isSelected = value;
                if (value)
                {
                    BackGround.color = SelectedColor;
                }
                else
                {
                    BackGround.color = Color.white;
                }
            }
        }

        public void Reset()
        {
            Icon.sprite = null;
            Name.text = string.Empty;
            IsSelected = false;
            Path = string.Empty;
            OnClickBtn.onClick.RemoveAllListeners();
        }
        public void Set(string name, string _path = null, System.Action<string> OnClickEvent = null, Sprite icon = null)
        {

        }
        private void Start()
        {

        }
    }
}
