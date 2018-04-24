// ==============================
// @author Nimanji (Indies a.k.a)
// ==============================

using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

using Assets.Script.Const;

// ==============================
// PuzzleSceneView
// ==============================
namespace Assets.Script.View
{
    public class PuzzleSceneView
    {
        // 各種レイヤーの作成
        private GameObject lay_background;
        private GameObject lay_main;
        private GameObject lay_ui;

        // ピクセル画像データの作成
        private Dictionary<string, Sprite> sprite_pixel = new Dictionary<string, Sprite>();

        // 生成されたピクセルの格納領域
        private Dictionary<string, GameObject> pixel_object = new Dictionary<string, GameObject>();

        /// <summary>
        /// PuzzleSceneView Construct
        /// </summary>
        public PuzzleSceneView()
        {
            // 各種レイヤーを取得する
            this.lay_background = GameObject.Find("Background");
            this.lay_main       = GameObject.Find("MainLayer");
            this.lay_ui         = GameObject.Find("UI");

            // 各種レイヤーにGameObjectを配置していく
            this.initBackgroundLayer();
            this.initMainlayer();
        }

        /// <sumamry>
        /// Backgroundに絵を配置する
        /// </summary>
        private void initBackgroundLayer()
        {
            // 画像データを取得
            Object bg_image = Resources.Load("Image/background", typeof(Sprite));
            
            // 取得した画像の名前でGameObjectを作成する
            GameObject instance_object = new GameObject(bg_image.name);
            instance_object.transform.SetParent(this.lay_background.transform, false);
            // RectTransformとImageを設定
            instance_object.AddComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
            instance_object.GetComponent<RectTransform>().localScale = new Vector2(1, 1);
            instance_object.AddComponent<Image>().sprite = (Sprite)bg_image;
            instance_object.GetComponent<Image>().preserveAspect = true;
            instance_object.GetComponent<Image>().SetNativeSize();
        }

        /// <summary>
        /// MainLayerにピクロスのマス目を配置する
        /// 配置するピクセルの数およびピクセル配置エリアは縦横ともに同値とする
        /// </summary>
        private void initMainlayer()
        {
            // ピクセルのON/OFF画像を読み込む
            Object[] pixel_image = Resources.LoadAll("Image/Pixel", typeof(Sprite));
            foreach (Sprite sprite in pixel_image) {
                this.sprite_pixel[sprite.name] = sprite;
            }

            // ピクセルの配置先となるオブジェクトを取得
            GameObject pixel_area = GameObject.Find("MainLayer/PixelArea");

            // ピクセル配置エリアを設置数で割り、1ピクセルの大きさを決定する
            int area_size = (int)pixel_area.GetComponent<RectTransform>().sizeDelta.x;
            float set_pixel_size = area_size / PlaySceneConst.SET_PIXEL_ROW_NUM;
            float set_pixel_size_scale = set_pixel_size / this.sprite_pixel["unpush"].texture.width;

            // ピクセルを配置していく
            float start_x = -250 + (set_pixel_size/2);
            float start_y =  250 - (set_pixel_size/2);
            for(int y = 0; y < PlaySceneConst.SET_PIXEL_ROW_NUM; y++) {
                for(int x = 0; x < PlaySceneConst.SET_PIXEL_ROW_NUM; x++) {
                    // 生成するピクセルの配置先を決定
                    Vector2 set_position = new Vector2(start_x + (set_pixel_size * x), start_y - (set_pixel_size * y));
                    // ピクセルを生成
                    GameObject pixel = new GameObject((x+1)+"-"+(y+1));
                    pixel.transform.SetParent(pixel_area.transform, false);
                    pixel.AddComponent<RectTransform>().anchoredPosition = set_position;
                    pixel.GetComponent<RectTransform>().localScale = new Vector2(1*set_pixel_size_scale, 1*set_pixel_size_scale);
                    pixel.AddComponent<Image>().sprite = this.sprite_pixel[PlaySceneConst.PIXEL_NAME_UNPUSH];
                    pixel.GetComponent<Image>().preserveAspect = true;
                    pixel.GetComponent<Image>().SetNativeSize();
                    // 生成したピクセルを配置
                    this.pixel_object[pixel.name] = pixel;
                }
            }
        }

        /// <summary>
        /// 受け取ったGameObject名のunpush/pushを切り替える
        /// </summary>
        /// <param name="obj_name">変更するオブジェクト名</param>
        public string changePixelModeByUnpushAndPush(string obj_name)
        {
            GameObject target_object = this.pixel_object[obj_name];
            Image target_image = target_object.GetComponent<Image>();

            if (PlaySceneConst.PIXEL_NAME_PUSH == target_image.sprite.name) {
                target_object.GetComponent<Image>().sprite = this.sprite_pixel[PlaySceneConst.PIXEL_NAME_UNPUSH];
            } else if (PlaySceneConst.PIXEL_NAME_UNPUSH == target_image.sprite.name) {
                target_object.GetComponent<Image>().sprite = this.sprite_pixel[PlaySceneConst.PIXEL_NAME_PUSH];
            }

            this.pixel_object[obj_name] = target_object;

            return "";
        }

        /// <summary>
        /// ヒント数字の描画を行う
        /// </summary>
        /// <param name="hint_values">ヒント数字の配列</param>
        public void drawHintValue(int[,,] hint_values)
        {
            // 行列の配置エリアを取得する
            GameObject row_area = GameObject.Find("MainLayer/HintAreaRow");
            GameObject col_area = GameObject.Find("MainLayer/HintAreaCol");

            // 各行列のヒント数字表示エリアを作成する
            Vector2 row_size = new Vector2(row_area.GetComponent<RectTransform>().sizeDelta.x, row_area.GetComponent<RectTransform>().sizeDelta.y/PlaySceneConst.SET_PIXEL_ROW_NUM);
            Vector2 col_size = new Vector2(col_area.GetComponent<RectTransform>().sizeDelta.x/PlaySceneConst.SET_PIXEL_ROW_NUM, col_area.GetComponent<RectTransform>().sizeDelta.y);
            // 行部分の作成
            Dictionary<int, GameObject> row_hint_area = new Dictionary<int, GameObject>();
            int key = 0;
            int row_set_y = Mathf.FloorToInt((row_size.y*PlaySceneConst.SET_PIXEL_ROW_NUM/2)-(row_size.y/2));
            for (int i = 0; i < PlaySceneConst.SET_PIXEL_ROW_NUM; i++) {
                GameObject row_object = new GameObject("RowHintArea"+(i+1));
                row_object.transform.SetParent(row_area.transform, false);
                row_object.AddComponent<RectTransform>().anchoredPosition = new Vector2(0, row_set_y);
                row_object.GetComponent<RectTransform>().sizeDelta = row_size;
                row_set_y -= (int)row_size.y;
                row_hint_area[key] = row_object;
                key++;
            }
            string row_text = "";
            for (int r = 0; r < PlaySceneConst.SET_PIXEL_ROW_NUM; r++) {
                for (int rp = 0; rp < PlaySceneConst.SET_PIXEL_ROW_NUM; rp++) {
                    if (0 < hint_values[1,r,rp]) {
                        // 1以上の数値を取得したら、テキストに追加する
                        row_text += hint_values[1,r,rp].ToString()+" ";
                    }
                }
                // 生成が完了した文字列を対象エリアに設定する
                row_hint_area[r] = this.setTextByAnyGameObject(row_hint_area[r], row_text, TextAnchor.MiddleRight);
                row_text = "";
            }
            // 列部分の作成
            Dictionary<int, GameObject> col_hint_area = new Dictionary<int, GameObject>();
            key = 0;
            int col_set_x = Mathf.FloorToInt((col_size.x*PlaySceneConst.SET_PIXEL_ROW_NUM/2)-(col_size.x/2))*(-1);
            for (int i = 0; i < PlaySceneConst.SET_PIXEL_ROW_NUM; i++) {
                GameObject col_object = new GameObject("ColHintArea"+(i+1));
                col_object.transform.SetParent(col_area.transform, false);
                col_object.AddComponent<RectTransform>().anchoredPosition = new Vector2(col_set_x, 0);
                col_object.GetComponent<RectTransform>().sizeDelta = col_size;
                col_set_x += (int)col_size.x;
                col_hint_area[key] = col_object;
                key++;
            }
            string col_text = "";
            for (int c = 0; c < PlaySceneConst.SET_PIXEL_ROW_NUM; c++) {
                for (int cp = 0; cp < PlaySceneConst.SET_PIXEL_ROW_NUM; cp++) {
                    if (0 < hint_values[0,c,cp]) {
                        // 1件以上の数値を取得したら、テキストに追加する
                        col_text += hint_values[0,c,cp].ToString()+"\n";
                    }
                }
                col_hint_area[c] = this.setTextByAnyGameObject(col_hint_area[c], col_text, TextAnchor.LowerCenter);
                col_text = "";
            }
        }

        /// <summary>
        /// 対象オブジェクトにテキストを持たせて表示させる
        /// </summary>
        /// <param name="target_object">テキストを付与させるGameObject</param>
        /// <param name="set_text">表示させるテキスト</param>
        /// <param name="text_anchor">テキスト揃え指定</param>
        private GameObject setTextByAnyGameObject(GameObject target_object, string set_text, TextAnchor text_anchor)
        {
            target_object.AddComponent<Text>().text = set_text;
            target_object.GetComponent<Text>().font = Resources.GetBuiltinResource (typeof(Font), "Arial.ttf") as Font;
            target_object.GetComponent<Text>().fontSize = 40;
            target_object.GetComponent<Text>().alignment = text_anchor;
            target_object.GetComponent<Text>().color = Color.black;

            return target_object;
        }
    }
}