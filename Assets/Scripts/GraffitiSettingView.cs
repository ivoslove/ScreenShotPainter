using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GraffitiSettingView : MonoBehaviour
{
    public GameObject ViewRootNode;
    public Slider BrushSizeSlider;
    public Slider EraserSizeSlider;
    public Image BrushUIIcon;
    public Image EraserUIIcon;
    public Button SaveButton;

    public Button EraserButton;
    public Button PaintButton;
    public Text Tip;



    [Range(0,1)]
    public float DefaultBrushSliderValue;
    [Range(0, 1)]
    public float DefaultEraserSliderValue;

    public Color DefaultBrushColor;
    public RawImage EraserIcon;
	void Start () {
        DefaultBrushSliderValue = 1-(PaintingParams.BrushDefaultSize- PaintingParams.BrushSizeMaxValue) /PaintingParams.BrushSizeFactor;
        DefaultEraserSliderValue = 1 - (PaintingParams.EraserDefaultSize - PaintingParams.EraserSizeMaxValue) / PaintingParams.EraserSizeFactor;


        DefaultBrushColor = new Color(0, 0, 0, 1);
        ScreenShotPainter.instance.FinishedRegionEvent.AddListener(() =>
        {
            ViewRootNode.SetActive(true);
    
        });
        ScreenShotPainter.instance.CannelRegionEvent.AddListener(() =>
        {
            ViewRootNode.SetActive(false);
        });
        ScreenShotPainter.instance.FinishedCapture.AddListener(() =>
        {
            ViewRootNode.SetActive(false);
            Tip.gameObject.SetActive(false);
        });
        ScreenShotPainter.instance.EnterCaptureModeEvent.AddListener(() =>
        {
            InitBrushUI();
            InitEraserUI();
            Tip.gameObject.SetActive(true);
           
        });
        ScreenShotPainter.instance.EscapeCaptureEvent.AddListener(() =>
        {
            ViewRootNode.SetActive(false);
            Tip.gameObject.SetActive(false);
            EraserIcon.gameObject.SetActive(false);
        });
        ScreenShotPainter.instance.EraserModeUpdateEvent.AddListener(EraserIconUpdate);
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void OnBtnOnClickChangeBrushColor(GameObject btnGameObject)
    {

        Color color = btnGameObject.GetComponent<Image>().color;
        BrushUIIcon.material.SetColor("_Color", color);
        ScreenShotPainter.instance.SetPaintColor(color);
    

    }

    public void OnBrushSliderValueChange()
    {

        float size = PaintingParams.BrushSizeMaxValue + (1-BrushSizeSlider.value) * PaintingParams.BrushSizeFactor;
        //ScreenShotPainter.instance.SetPaintingSize(size);

        float sizeY = ((float)Screen.height / (float)Screen.width) * size;

        ScreenShotPainter.instance.SetPaintingSize(size, sizeY);

        float width = Screen.width / size;
        float height = Screen.width / size;//不缩放y轴了
        BrushUIIcon.transform.localScale = new Vector3(width / BrushUIIcon.rectTransform.rect.width, height / BrushUIIcon.rectTransform.rect.height, 1);

    }

    public void OnEraserSliderValueChange()
    {

        float size = PaintingParams.EraserSizeMaxValue + (1 - EraserSizeSlider.value) * PaintingParams.EraserSizeFactor;

        float sizeY = ((float)Screen.height / (float)Screen.width) * size;

        ScreenShotPainter.instance.SetEraserSize(size, sizeY);

        float width = Screen.width / size;
        float height = Screen.width / size;//不缩放y轴了
        EraserUIIcon.transform.localScale = new Vector3(width / EraserUIIcon.rectTransform.rect.width, height / EraserUIIcon.rectTransform.rect.height, 1);

    }
    public void SaveScreenShot()
    {
        ScreenShotPainter.instance.SaveCapture();
        ViewRootNode.SetActive(false);
        Tip.gameObject.SetActive(false);
    }

    public void ChangeToEraserBtn()
    {
        BrushSizeSlider.gameObject.SetActive(false);
        BrushUIIcon.gameObject.SetActive(false);
        EraserSizeSlider.gameObject.SetActive(true);
        EraserUIIcon.gameObject.SetActive(true);
        ScreenShotPainter.instance.ChangeToEraser(true);
        
    }
    public void ChangeToPaintBtn()
    {
        BrushSizeSlider.gameObject.SetActive(true);
        BrushUIIcon.gameObject.SetActive(true);
        EraserSizeSlider.gameObject.SetActive(false);
        EraserUIIcon.gameObject.SetActive(false);
        ScreenShotPainter.instance.ChangeToEraser(false);
    }

    public void EraserIconUpdate(bool show)
    {
        if (show)
        {
            EraserIcon.rectTransform.anchoredPosition=new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            //大小
            float size = PaintingParams.EraserSizeMaxValue + (1 - EraserSizeSlider.value) * PaintingParams.EraserSizeFactor;
            float width = Screen.width/ size;
            float height = Screen.width / size;//不缩放y轴了
            EraserIcon.transform.localScale=new Vector3(width/EraserIcon.rectTransform.rect.width, height / EraserIcon.rectTransform.rect.height,1);
            //EraserIcon.transform.localScale=new Vector3(300/width);
            EraserIcon.gameObject.SetActive(true);
        }
        else
        {
            EraserIcon.gameObject.SetActive(false);
        }
    }
    /// <summary>
    /// 初始化画笔
    /// </summary>
    public void InitBrushUI()
    {
        BrushUIIcon.material.SetColor("_Color", DefaultBrushColor);
        ScreenShotPainter.instance.SetPaintColor(DefaultBrushColor);

        BrushSizeSlider.value = DefaultBrushSliderValue;
        float size = PaintingParams.BrushSizeMaxValue + (1 - DefaultBrushSliderValue) * PaintingParams.BrushSizeFactor;
        //ScreenShotPainter.instance.SetPaintingSize(size);

        float sizeY = ((float)Screen.height / (float)Screen.width) * size;

 
        ScreenShotPainter.instance.SetPaintingSize(size, sizeY);

        float width = Screen.width / size;
        float height = Screen.width / size;//不缩放y轴了
        BrushUIIcon.transform.localScale = new Vector3(width / BrushUIIcon.rectTransform.rect.width, height / BrushUIIcon.rectTransform.rect.height, 1);

        BrushSizeSlider.gameObject.SetActive(true);
        BrushUIIcon.gameObject.SetActive(true);
    }
    /// <summary>
    /// 初始化橡皮擦
    /// </summary>
    public void InitEraserUI()
    {
        EraserSizeSlider.value = DefaultEraserSliderValue;
        float size = PaintingParams.EraserSizeMaxValue + (1 - DefaultEraserSliderValue) * PaintingParams.EraserSizeFactor;
        //ScreenShotPainter.instance.SetPaintingSize(size);

        float sizeY = ((float)Screen.height / (float)Screen.width) * size;

        ScreenShotPainter.instance.SetEraserSize(size, sizeY);

        float width = Screen.width / size;
        float height = Screen.width / size;//不缩放y轴了
        EraserUIIcon.transform.localScale = new Vector3(width / EraserUIIcon.rectTransform.rect.width, height / EraserUIIcon.rectTransform.rect.height, 1);
        EraserUIIcon.gameObject.SetActive(false);
        EraserSizeSlider.gameObject.SetActive(false);
        
    }
}
