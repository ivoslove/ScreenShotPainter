using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Camera))]
public class ScreenShotPainter : MonoBehaviour {

    public static ScreenShotPainter instance;

    #region private numbers

    //[SerializeField] 
    private string _captureSavePath;

    private Canvas _paintCanvas;

    //[SerializeField]
    private Texture _defaultBrushRawImage;

    /// <summary>
    /// 涂鸦的RenderTexture图片
    /// </summary>
    private RenderTexture _blitRenderTexture;
    /// <summary>
    /// 截图灰化背景图
    /// </summary>
    //[SerializeField]
    private RawImage _bgRawImage;

    //绘图shader&material
    //[SerializeField]
    private Shader _paintBrushShader;
    private Material _paintBrushMat;

    //清理renderTexture的shader&material
    //[SerializeField]
    private Shader _clearBrushShader;
    private Material _clearBrushMat;
    //[SerializeField]
    private Shader _eraserBrushShader;
    private Material _eraserBrushMat;
    //笔刷的默认颜色
    [SerializeField]
    private Color _defaultColor= Color.black;

    //涂鸦的画布
    [SerializeField]
    private RawImage _paintCanvasImg;

    /// <summary>
    /// 笔刷的大小
    /// </summary>
    [SerializeField]
    private float _brushSize=300f;
    /// <summary>
    /// 橡皮擦的大小
    /// </summary>
    [SerializeField]
    private float _eraserSize = 300f;
    /// <summary>
    /// 笔刷间隔
    /// </summary>
    private float _brushLerpSize;
    /// <summary>
    /// 默认上一次点的位置
    /// </summary>
    private Vector2 _lastPoint;

    /// <summary>
    /// 截图框选左下角点坐标
    /// </summary>
    private Vector2 _leftDownConnerPoint;
    /// <summary>
    /// 截图框选右上角点坐标
    /// </summary>
    private Vector2 _rightUpConnerPoint;
    /// <summary>
    /// 截图框选左上角点坐标
    /// </summary>
    private Vector2 _leftUpConnerPoint;
    /// <summary>
    /// 截图框选右下角点坐标
    /// </summary>
    private Vector2 _rightDownConnerPoint;

    //屏幕的宽高
    private int _screenWidth;
    private int _screenHeight;

 
    #region 框选截图区域
    //框选截图起始点
    private Vector2 _startPoint;
    private Vector2 _endPoint;
    //框的颜色
    [SerializeField]
    private Color _rectColor = Color.red;

    private bool _haveCirmformRectStarPoint;

    #endregion
    /// <summary>
    /// 组件开关
    /// </summary>
    private bool _enabled;

    private bool _eraserFlag;
    private CaptureType _defaultCaptureType;
    /// <summary>
    /// 是否已经选了区域
    /// </summary>
    private bool _haveRegion;

    private bool _drawRegionRect;
    /// <summary>
    /// 截图标记位
    /// </summary>
    private bool _captureFlag;
    /// <summary>
    /// 是否要截取UI
    /// </summary>
    [SerializeField]
    private bool _captureWithUI;

    private Material _lineMaterial;


    #region 一些外部注册事件
    /// <summary>
    /// 画完区域后要做的事情（其实就是处理UI面板，给外部调用）
    /// </summary>
    private  FinishedRegionEvent _finishedRegionEvent=new FinishedRegionEvent();
    /// <summary>
    /// 取消选择区域后要做的事情（其实就是处理UI面板，给外部调用）
    /// </summary>
    private CannelRegionEvent _cannelRegionEvent=new CannelRegionEvent();

    private FinishedCaptureEvent _finishedCapture=new FinishedCaptureEvent();

    private EscapeCaptureEvent _escapeCaptureEvent=new EscapeCaptureEvent();

    private EnterCaptureModeEvent _enterCaptureModeEvent=new EnterCaptureModeEvent();

    private EraserModeUpdateEvent _eraserModeUpdateEvent=new EraserModeUpdateEvent();
    #endregion
    #endregion




    #region public properties




    public FinishedRegionEvent FinishedRegionEvent
    {
        get { return _finishedRegionEvent; }
    }

    public CannelRegionEvent CannelRegionEvent
    {
        get { return _cannelRegionEvent; }
    }

    public FinishedCaptureEvent FinishedCapture
    {
        get { return _finishedCapture; }
    }

    public EscapeCaptureEvent EscapeCaptureEvent
    {
        get { return _escapeCaptureEvent; }
    }
    #endregion

    public EnterCaptureModeEvent EnterCaptureModeEvent
    {
        get { return _enterCaptureModeEvent; }
    }

    public EraserModeUpdateEvent EraserModeUpdateEvent
    {
        get { return _eraserModeUpdateEvent; }
    }
    #region life circle

    void Awake()
    {
        instance = this;

        if (_paintCanvas == null)
        {
            GameObject canvasGameObject = Resources.Load<GameObject>("PaintCanvas");
            _paintCanvas = GameObject.Instantiate(canvasGameObject).GetComponent<Canvas>();
            _paintCanvas.sortingOrder = 5;
            _bgRawImage = _paintCanvas.transform.Find("captureBGImg").GetComponent<RawImage>();
            _paintCanvasImg= _paintCanvas.transform.Find("paintCanvasImg").GetComponent<RawImage>();
        }
         _captureWithUI = true;
        _captureSavePath = Application.dataPath;
        _defaultCaptureType = CaptureType.FreeRegion;

        //_paintCanvasImg = GameObject.FindObjectOfType<Canvas>().transform.Find("paintCanvasImg").GetComponent<RawImage>();
        _screenWidth = Screen.width;
        _screenHeight = Screen.height;
        _paintBrushShader=Resources.Load<Shader>("Shaders/PaintBrush");
        _paintBrushMat = new Material(_paintBrushShader);
        _clearBrushShader = Resources.Load<Shader>("Shaders/ClearBrush");
        _clearBrushMat = new Material(_clearBrushShader);
        _eraserBrushShader= Resources.Load<Shader>("Shaders/EraserBrush");
        _eraserBrushMat=new Material(_eraserBrushShader);

        _defaultBrushRawImage = Resources.Load<Texture>("brush-1");

        //初始化刷子
        _paintBrushMat.SetTexture("_BrushTex", _defaultBrushRawImage);
        _paintBrushMat.SetColor("_Color", _defaultColor);
        _brushSize = PaintingParams.BrushDefaultSize;
        _brushLerpSize = (_defaultBrushRawImage.width + _defaultBrushRawImage.height) / 2.0f / _brushSize;
        _paintBrushMat.SetFloat("_Size", _brushSize);
        _paintBrushMat.SetFloat("_SizeY", _brushSize * (float)_screenHeight / (float)_screenWidth);

        _eraserSize= PaintingParams.EraserDefaultSize;
        _eraserBrushMat.SetTexture("_BrushTex", _defaultBrushRawImage);
        _eraserBrushMat.SetColor("_Color", new Color(0f, 0f, 0f, 0f));
        _eraserBrushMat.SetFloat("_Size", _eraserSize);
        _eraserBrushMat.SetFloat("_SizeY", _eraserSize * (float)_screenHeight / (float)_screenWidth);

        _blitRenderTexture = RenderTexture.GetTemporary(_screenWidth, _screenHeight, 24);
        _paintCanvasImg.texture = _blitRenderTexture;
        //给画布添加事件
        var paintEventTrigger = _paintCanvasImg.GetComponent<EventTrigger>();
        if (paintEventTrigger == null)
        {
            paintEventTrigger = _paintCanvasImg.gameObject.AddComponent<EventTrigger>();
            paintEventTrigger.triggers = new List<EventTrigger.Entry>();

            
                EventTrigger.Entry dragEntry = new EventTrigger.Entry();
                dragEntry.eventID = EventTriggerType.Drag;
                dragEntry.callback.AddListener(PaintDragging);

                EventTrigger.Entry endDragEntry = new EventTrigger.Entry();
                endDragEntry.eventID = EventTriggerType.EndDrag;
                endDragEntry.callback.AddListener(OnPaintEndDrag);
                paintEventTrigger.triggers.Add(dragEntry);
                paintEventTrigger.triggers.Add(endDragEntry);
        }
        Graphics.Blit(_blitRenderTexture, _blitRenderTexture, _clearBrushMat);

    }

    void OnEnable()
    {
      
    }
    void Start()
    {

    }

    void Update()
    {
        if (_enabled)
        {
            switch (_defaultCaptureType)
            {
                case CaptureType.FullScreen:
                    break;
                case CaptureType.FreeRegion:
                    //已经选了区域，可以开始涂鸦了
                    if (_haveRegion)
                    {
                        //如果按下右键，表示重新选择区域,清除掉之前的记录
                        if (Input.GetMouseButtonUp(1))
                        {
                            _defaultCaptureType = CaptureType.FreeRegion;
                            _haveRegion = false;
                            _haveCirmformRectStarPoint = false;
                            _startPoint = Vector2.zero;
                            _endPoint = Vector2.zero;
                            _drawRegionRect = false;
                            _leftUpConnerPoint = Vector2.zero;
                            _leftDownConnerPoint = Vector2.zero;
                            _rightUpConnerPoint = Vector2.zero;
                            _rightDownConnerPoint = Vector2.zero;
                            _blitRenderTexture.Release();
                            _bgRawImage.material.SetVector("_Rect", new Vector4(0, 0, 0, 0));
                            _eraserFlag = false;
                            //Graphics.Blit(_blitRenderTexture, _blitRenderTexture, _clearBrushMat); //清除一下
                            CannelRegionEvent.Invoke();
                            return;
                        }
                        //涂鸦的侦听逻辑


                    }
                    //未选择区域，要先选区域
                    else
                    {

                        //switch (_defaultCaptureType)
                        //{
                        //    case CaptureType.FullScreen:
                        //        break;
                        //    case CaptureType.FreeRegion:
                        //        //如果没有点下选择起点
                        if (!_haveCirmformRectStarPoint)
                        {
                            if (Input.GetMouseButtonDown(0))
                            {
                                //超出屏幕外
                                if (Input.mousePosition.x < 0 || Input.mousePosition.x > Screen.width ||
                                    Input.mousePosition.y < 0 || Input.mousePosition.y > Screen.height)
                                {
                                    return;
                                }
                                _startPoint = Input.mousePosition;
                                _haveCirmformRectStarPoint = true;

                            }
                        }
                        //如果选下了起点，刷新鼠标位置
                        else
                        {
                            _drawRegionRect = true;
                            if (Input.GetMouseButton(0))
                            {
                                _endPoint = Input.mousePosition;
                                //限制坐标在屏幕内
                                _endPoint.x = Mathf.Clamp(_endPoint.x, 0f, Screen.width);
                                _endPoint.y = Mathf.Clamp(_endPoint.y, 0f, Screen.height);
                                //设置灰化区域
                                Vector4 rect = new Vector4(Mathf.Min(_startPoint.x, _endPoint.x) / Screen.width,
                                    Mathf.Min(_startPoint.y, _endPoint.y) / Screen.height,
                                    Mathf.Max(_startPoint.x, _endPoint.x) / Screen.width,
                                    Mathf.Max(_startPoint.y, _endPoint.y) / Screen.height);
                                _bgRawImage.material.SetVector("_Rect", rect);
                            }
                            //鼠标抬起，截图框选区域确定
                            if (Input.GetMouseButtonUp(0))
                            {
                                _endPoint = Input.mousePosition;
                                //限制坐标在屏幕内
                                _endPoint.x = Mathf.Clamp(_endPoint.x, 0f, Screen.width);
                                _endPoint.y = Mathf.Clamp(_endPoint.y, 0f, Screen.height);

                                _haveRegion = true;
                                _leftUpConnerPoint = GetCaptureViewLeftUpConnerPoint();
                                _rightUpConnerPoint = GetCaptureViewRightUpConnerPoint();
                                _rightDownConnerPoint = GetCaptureViewRightDownConnerPoint();
                                _leftDownConnerPoint = GetCaptureViewLeftDownConnerPoint();
                                FinishedRegionEvent.Invoke();
                            }
                        }

                        //        break;
                        //    default:
                        //        throw new ArgumentOutOfRangeException();
                        //}
                    }
                    ////侦听截图事件
                    //if (Input.GetKeyUp(KeyCode.Space))
                    //{
                    //    _CaptureFlag = true;
                    //}
                    //全屏截图
                    if (Input.GetKeyUp(KeyCode.F) && _defaultCaptureType == CaptureType.FreeRegion && !_haveRegion)
                    {
                        _defaultCaptureType = CaptureType.FullScreen;
                        _leftDownConnerPoint = Vector2.zero;
                        _leftUpConnerPoint = new Vector2(0, _screenHeight);
                        _rightUpConnerPoint = new Vector2(_screenWidth, _screenHeight);
                        _rightDownConnerPoint = new Vector2(_screenWidth, 0);
                        _drawRegionRect = true;
                        _haveCirmformRectStarPoint = true;
                        _haveRegion = true;
                      
                        _bgRawImage.gameObject.SetActive(false);
                        _bgRawImage.material.SetVector("_Rect", new Vector4(0, 0, 1, 1));
                        //涂鸦选项UI初始化
                        FinishedRegionEvent.Invoke();
                    }
                    break;
                default:
                  break;
            }


            _eraserModeUpdateEvent.Invoke(_eraserFlag);


            //退出截图
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                ResetData();
                _enabled = false;
             
                _escapeCaptureEvent.Invoke();
                return;
            }
        }
    }

    void OnPostRender()
    {
        if(!enabled)
            return;
        if (_drawRegionRect)
        {

            //如果材质球不存在
            if (!_lineMaterial)
            {
                //实例一个材质球
                Shader shader = Shader.Find("Hidden/Internal-Colored");
                _lineMaterial = new Material(shader);
                _lineMaterial.hideFlags = HideFlags.HideAndDontSave;
                //设置参数
                _lineMaterial.SetInt("_SrcBlend", (int) UnityEngine.Rendering.BlendMode.SrcAlpha);
                _lineMaterial.SetInt("_DstBlend", (int) UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                //设置参数
                _lineMaterial.SetInt("_Cull", (int) UnityEngine.Rendering.CullMode.Off);
                //设置参数
                _lineMaterial.SetInt("_ZWrite", 0);

            }

            GL.PushMatrix();//保存摄像机变换矩阵,把投影视图矩阵和模型视图矩阵压入堆栈保

            GL.LoadPixelMatrix();//设置用屏幕坐标绘图

            _lineMaterial.SetPass(0);
            GL.Begin(GL.LINES);
            GL.Color(_rectColor);

            switch (_defaultCaptureType)
            {
                case CaptureType.FullScreen:
                    GL.Vertex3(_leftUpConnerPoint.x, _leftUpConnerPoint.y, 0);
                    GL.Vertex3(_rightUpConnerPoint.x, _rightUpConnerPoint.y, 0);

                    GL.Vertex3(_rightUpConnerPoint.x, _rightUpConnerPoint.y, 0);
                    GL.Vertex3(_rightDownConnerPoint.x, _rightDownConnerPoint.y, 0);

                    GL.Vertex3(_rightDownConnerPoint.x, _rightDownConnerPoint.y, 0);
                    GL.Vertex3(_leftDownConnerPoint.x, _leftDownConnerPoint.y, 0);

                    GL.Vertex3(_leftDownConnerPoint.x, _leftDownConnerPoint.y, 0);
                    GL.Vertex3(_leftUpConnerPoint.x, _leftUpConnerPoint.y, 0);
                    break;
                case CaptureType.FreeRegion:
                    GL.Vertex3(_startPoint.x, _startPoint.y, 0);
                    GL.Vertex3(_endPoint.x, _startPoint.y, 0);

                    GL.Vertex3(_endPoint.x, _startPoint.y, 0);
                    GL.Vertex3(_endPoint.x, _endPoint.y, 0);

                    GL.Vertex3(_endPoint.x, _endPoint.y, 0);
                    GL.Vertex3(_startPoint.x, _endPoint.y, 0);

                    GL.Vertex3(_startPoint.x, _endPoint.y, 0);
                    GL.Vertex3(_startPoint.x, _startPoint.y, 0);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

         
           

            GL.End();
            GL.PopMatrix();//恢复摄像机投影矩阵
        }

        //截图
        if (_captureFlag&&!_captureWithUI&&_haveRegion)
        {
            _drawRegionRect = false;
            float width = _rightUpConnerPoint.x - _leftDownConnerPoint.x;
            float height = _rightUpConnerPoint.y - _leftDownConnerPoint.y;
            Debug.LogError(_leftDownConnerPoint.x.ToString("0.00"));
            Debug.LogError(_leftDownConnerPoint.y.ToString("0.00"));
            Debug.LogError("width:" + width);
            Debug.LogError("width:" + height);
            Rect rect = new Rect(_leftDownConnerPoint.x, Screen.height - _leftUpConnerPoint.y, width, height);


            Texture2D tex1 = new Texture2D((int)width, (int)height, TextureFormat.RGB24, false);
            //Read the pixels in the Rect starting at 0,0 and ending at the screen's width and height
            tex1.ReadPixels(rect, 0, 0, false);
            tex1.Apply();
            //Check that the display field has been assigned in the Inspector
#if UNITY_EDITOR
            byte[] tex1bytes = tex1.EncodeToPNG();

            string tex1path = _captureSavePath + "/RenderTextureActive.png";
            System.IO.File.WriteAllBytes(tex1path, tex1bytes);
            Debug.Log(string.Format("截屏了一张照片: {0}", tex1path));

#endif
            //Texture2D CombineTex = new Texture2D(_renderTex.width, _renderTex.height, TextureFormat.ARGB32, false);
            RenderTexture prev = RenderTexture.active;
            RenderTexture.active = _blitRenderTexture;
            Texture2D BliTexture2D = new Texture2D((int)width, (int)height, TextureFormat.ARGB32, false);



            BliTexture2D.ReadPixels(rect, 0, 0);

#if UNITY_EDITOR
            byte[] bytes = BliTexture2D.EncodeToPNG();

            string path = _captureSavePath + "/BlitTexture.png";

            string filename = path;
            System.IO.File.WriteAllBytes(filename, bytes);
            Debug.Log(string.Format("截屏了一张照片: {0}", filename));
#endif
            RenderTexture.active = null;

            Texture2D combineTexture2D = MergeImage(tex1, BliTexture2D);
            byte[] combineTexture2DbBytes = combineTexture2D.EncodeToPNG();
            string str = DateTime.Now.ToString("hh_mm_ss");
            string fileName = DateTime.Now.ToString("yyyy_MM_dd") + "_" + str + ".png";
            string path2 = _captureSavePath + "/"+ fileName;

            System.IO.File.WriteAllBytes(path2, combineTexture2DbBytes);
            Debug.Log(string.Format("截屏了一张照片: {0}", path2));

            _captureFlag = false;
            //重置数据
            ResetData();
            _finishedCapture.Invoke();
        }
        //UI也截取
        if (_captureFlag && _captureWithUI&& _haveRegion)
        {
            StartCoroutine(StartCapture());
        }

    }

    #endregion


    #region private function

    #region 涂鸦相关方法

    private void PaintDragging(BaseEventData data)
    {
        if (_enabled && _haveRegion)
        {
            if (Input.GetMouseButton(0))
                LerpPaint(Input.mousePosition);
        }
    }
    private void OnPaintEndDrag(BaseEventData data)
    {
        if (_enabled && _haveRegion)
        {
            if (Input.GetMouseButtonUp(0))
                _lastPoint = Vector2.zero;
        }
    }
    /// <summary>
    /// 绘画进行插值
    /// </summary>
    /// <param name="point"></param>
    private void LerpPaint(Vector2 point)
    {
        Paint(point);

        if (_lastPoint == Vector2.zero)
        {
            _lastPoint = point;
            return;
        }

        float dis = Vector2.Distance(point, _lastPoint);
        if (dis > _brushLerpSize)
        {
            Vector2 dir = (point - _lastPoint).normalized;
            int num = (int)(dis / _brushLerpSize);
            for (int i = 0; i < num; i++)
            {
                Vector2 newPoint = _lastPoint + dir * (i + 1) * _brushLerpSize;
                Paint(newPoint);
            }
        }
        _lastPoint = point;
    }
    //画点
    private void Paint(Vector2 point)
    {

        if (point.x < _leftDownConnerPoint.x || point.x > _rightUpConnerPoint.x || point.y < _leftDownConnerPoint.y || point.y > _rightUpConnerPoint.y)
            return;

        Vector2 uv = new Vector2(point.x / (float)_screenWidth,
            point.y / (float)_screenHeight);
        if (_eraserFlag)
        {
            _eraserBrushMat.SetVector("_UV", uv);
            Graphics.Blit(_blitRenderTexture, _blitRenderTexture, _eraserBrushMat);
        }
        else
        {
            _paintBrushMat.SetVector("_UV", uv);
            Graphics.Blit(_blitRenderTexture, _blitRenderTexture, _paintBrushMat);
        }

    }


    public Vector2 GetCaptureViewLeftDownConnerPoint()
    {
        Vector2 vec = new Vector2(Mathf.Min(_startPoint.x, _endPoint.x), Mathf.Min(_startPoint.y, _endPoint.y));
        return vec;
    }
    public Vector2 GetCaptureViewRightUpConnerPoint()
    {
        Vector2 vec = new Vector2(Mathf.Max(_startPoint.x, _endPoint.x), Mathf.Max(_startPoint.y, _endPoint.y));
        return vec;
    }
    public Vector2 GetCaptureViewLeftUpConnerPoint()
    {
        Vector2 vec = new Vector2(Mathf.Min(_startPoint.x, _endPoint.x), Mathf.Max(_startPoint.y, _endPoint.y));
        return vec;
    }
    public Vector2 GetCaptureViewRightDownConnerPoint()
    {
        Vector2 vec = new Vector2(Mathf.Max(_startPoint.x, _endPoint.x), Mathf.Min(_startPoint.y, _endPoint.y));
        return vec;
    }
    #endregion

    /// <summary>
    /// 合成图片
    /// </summary>
    private Texture2D MergeImage(Texture2D tex1, Texture2D tex2)
    {

        for (int i = 0; i < tex1.width; i++)
        {
            for (int j = 0; j < tex1.height; j++)
            {
                Color color1 = tex1.GetPixel(i, j);
                Color color2 = tex2.GetPixel(i, j);
                Color newColor = color2.a * color2 + (1.0f - color2.a) * color1;
                newColor.a = 1f;
                tex1.SetPixel(i, j, newColor);

            }
        }

        return tex1;
    }

    IEnumerator StartCapture()
    {
        yield return new WaitForEndOfFrame();
        float width = _rightUpConnerPoint.x - _leftDownConnerPoint.x;
        float height = _rightUpConnerPoint.y - _leftDownConnerPoint.y;
        Debug.Log(_leftDownConnerPoint.x.ToString("0.00"));
        Debug.Log(_leftDownConnerPoint.y.ToString("0.00"));
        Debug.Log("width:" + width);
        Debug.Log("width:" + height);
        Rect rect = new Rect(_leftDownConnerPoint.x, _leftDownConnerPoint.y, width, height); //坑爹啊 如果是开携程在WaitForEndOfFrame时候截图，那么他的坐标系换了。。rect起始点要设置在左下角



        Texture2D tex1 = new Texture2D((int)width, (int)height, TextureFormat.RGB24, false);
        //Read the pixels in the Rect starting at 0,0 and ending at the screen's width and height
        tex1.ReadPixels(rect, 0, 0, false);
        tex1.Apply();
        //Check that the display field has been assigned in the Inspector
        byte[] tex1bytes = tex1.EncodeToPNG();

        string str = DateTime.Now.ToString("hh_mm_ss");
        string fileName= DateTime.Now.ToString("yyyy_MM_dd")+"_"+ str + ".png";
        string tex1path = _captureSavePath + "/"+ fileName;
        System.IO.File.WriteAllBytes(tex1path, tex1bytes);
        Debug.Log(string.Format("截屏了一张照片(有UI): {0}", tex1path));


        _captureFlag = false;
        ResetData();
        //推出截屏
        EscapeCaptureEvent.Invoke();
    }

    /// <summary>
    ///  截图完成后重置数据
    /// </summary>
    private void ResetData()
    {
        _defaultCaptureType = CaptureType.FreeRegion;
        _drawRegionRect = false;
        _haveRegion = false;
        _haveCirmformRectStarPoint = false;
        _blitRenderTexture.Release();
        _startPoint=Vector2.zero;
        _endPoint=Vector2.zero;
        _leftUpConnerPoint=Vector2.zero;
        _leftDownConnerPoint=Vector2.zero;
        _rightDownConnerPoint=Vector2.zero;
        _rightUpConnerPoint=Vector2.zero;
        _lastPoint=Vector2.zero;
        _bgRawImage.material.SetVector("_Rect", new Vector4(0, 0, 1, 1));
        _bgRawImage.gameObject.SetActive(false);
        _paintCanvasImg.gameObject.SetActive(false);
        _eraserFlag = false;
        _enabled = false;
    }
    #endregion

    #region public function
    /// <summary>
    /// 进入截图模式
    /// </summary>
    public void SwitchOn(bool isFullScreen)
    {
        if (isFullScreen)
        {
            _defaultCaptureType = CaptureType.FullScreen;
            _bgRawImage.gameObject.SetActive(false);
            _bgRawImage.material.SetVector("_Rect", new Vector4(0, 0, 1, 1));

            _leftDownConnerPoint = Vector2.zero;
            _leftUpConnerPoint = new Vector2(0, _screenHeight);
            _rightUpConnerPoint = new Vector2(_screenWidth, _screenHeight);
            _rightDownConnerPoint = new Vector2(_screenWidth, 0);
            _drawRegionRect = true;
            _haveCirmformRectStarPoint = true;
            _haveRegion = true;
            _lastPoint=Vector2.zero;
            //涂鸦选项UI初始化
            FinishedRegionEvent.Invoke();
        }
        else
        {

            _defaultCaptureType = CaptureType.FreeRegion;
            _bgRawImage.gameObject.SetActive(true);
            _bgRawImage.material.SetVector("_Rect", new Vector4(0, 0, 0, 0));
        }
        _paintCanvasImg.gameObject.SetActive(true);
        _enterCaptureModeEvent.Invoke();
        _enabled = true;
  
    }
    public void SaveCapture()
    {
        _captureFlag = true;
    }

    public void SetPaintColor(Color color)
    {
        _paintBrushMat.SetColor("_Color", color);
    }

    public void SetPaintingSize(float size)
    {
        _paintBrushMat.SetFloat("_Size", size);
        _brushLerpSize = (_defaultBrushRawImage.width + _defaultBrushRawImage.height) / 2.0f / size;
        _eraserBrushMat.SetFloat("_Size", size);
        _brushLerpSize = (_defaultBrushRawImage.width + _defaultBrushRawImage.height) / 2.0f / size;
    }

    public void SetPaintingSize(float sizeX, float sizeY)
    {
        _paintBrushMat.SetFloat("_Size", sizeX);
        _paintBrushMat.SetFloat("_SizeY", sizeY);
        if(sizeX>sizeY)
          _brushLerpSize = (_defaultBrushRawImage.width + _defaultBrushRawImage.height) / 2.0f / sizeX;
        if(sizeX <= sizeY)
         _brushLerpSize = (_defaultBrushRawImage.width + _defaultBrushRawImage.height) / 2.0f / sizeY;

    }
    public void SetEraserSize(float sizeX, float sizeY)
    {

        if (sizeX > sizeY)
            _brushLerpSize = (_defaultBrushRawImage.width + _defaultBrushRawImage.height) / 2.0f / sizeX;
        if (sizeX <= sizeY)
            _brushLerpSize = (_defaultBrushRawImage.width + _defaultBrushRawImage.height) / 2.0f / sizeY;
        _eraserBrushMat.SetFloat("_Size", sizeX);
        _eraserBrushMat.SetFloat("_SizeY", sizeY);
        //_brushLerpSize = (_defaultBrushRawImage.width + _defaultBrushRawImage.height) / 2.0f / size;
    }

    public void ChangeToEraser(bool toEraser)
    {
        _eraserFlag = toEraser;

    }

    #endregion

}

public class FinishedRegionEvent:UnityEvent
{

}

public class CannelRegionEvent : UnityEvent
{

}

public class FinishedCaptureEvent : UnityEvent
{

}
public class EscapeCaptureEvent : UnityEvent
{

}

public class EnterCaptureModeEvent : UnityEvent
{

}

public class EraserModeUpdateEvent : UnityEvent<bool>
{

}

public enum CaptureType
{
    /// <summary>
    /// 自动全屏截屏
    /// </summary>
    FullScreen,
    /// <summary>
    /// 自由框选截屏
    /// </summary>
    FreeRegion
}
/// <summary>
/// 笔刷橡皮擦计算参数 ，计算公式，比如笔刷的是【缩放倍数=BrushSizeMaxValue +（1-slider.value）*BrushSizeFactor】,橡皮擦类似
/// </summary>
public static class PaintingParams
{
    /// <summary>
    /// 缩小因子，越大说明可调节范围越大
    /// </summary>
    public static float BrushSizeFactor = 300f;
    /// <summary>
    /// 笔刷图片最大缩小倍数（画布大小的笔刷图案要缩小多少倍数显示，缩小的越小，笔刷显示效果越大）
    /// </summary>
    public static float BrushSizeMaxValue = 50;

    public static float  EraserSizeFactor = 50;

    public static float EraserSizeMaxValue = 50;

    public static float BrushDefaultSize=300;
    public static float EraserDefaultSize=100;
}