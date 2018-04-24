// ==============================
// @author Nimanji (Indies a.k.a)
// ==============================

using System.IO;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

using Assets.Script.Const;

// ==============================
// PuzzleSceneModel
// ==============================
namespace Assets.Script.Model
{
    public class PuzzleSceneModel
    {
        // CSVディレクトリまでのパス
        private string csv_path = Application.dataPath+"/Resources/CSV/";

        // View上に配置されたピクセルを格納
        private GameObject pixel_parent;
        private Dictionary<string, GameObject> pixel_children = new Dictionary<string, GameObject>();

        // 回答データの格納
        private Dictionary<string, bool> correct_data_dic = new Dictionary<string, bool>();
        private bool[,] correct_data;

        // 正解ピクセルの合計数と押された正解ピクセルの数
        private int total_correct_pixel_num;
        private int pushed_correct_pixel_num;

        /// <summary>
        /// PuzzleSceneModel Construct
        /// </summary>
        public PuzzleSceneModel()
        {
            // PixelAreaとその子オブジェクトをそれぞれを取得する
            this.pixel_parent = GameObject.Find("MainLayer/PixelArea");
            Transform tmp_child = this.pixel_parent.transform;
            foreach (Transform child in tmp_child) {
                this.pixel_children[child.name] = child.gameObject;
            }
            // 正解/不正解判定用の変数の初期化
            this.total_correct_pixel_num = 0;
            this.pushed_correct_pixel_num = 0;
            // 正解データをCSVから取得する
            this.getCorrectDataByCsvFile();
            this.createHintValueMatrix();
        }

        /// <summary>
        /// CSVデータから回答データを取得する
        /// </summary>
        public void getCorrectDataByCsvFile()
        {
            // CSVデータの読み込み
            StreamReader sr = new StreamReader(this.csv_path + "1-1.csv");
            string stream_text = sr.ReadToEnd();
            // 行で分割する
            string[] row = stream_text.Split(new char[]{'\n'});
            // 列分割の文字を設定
            char[] separate_text = new char[1]{','};
            // 行数と列数を取得
            int row_length = row.Length;
            int col_length = row[0].Split(separate_text).Length;
            // 回答データを格納する配列を作成
            this.correct_data = new bool[row_length, col_length];
            // 回答データを格納
            for (int c = 0; c < col_length; c++) {
                string[] tmp_rows = row[c].Split(separate_text);
                for (int r = 0; r < row_length; r++) {
                    int correct = int.Parse(tmp_rows[r]);
                    if (1 == correct) {
                        this.correct_data_dic[(r+1)+"-"+(c+1)] = true;
                        this.correct_data[r, c] = true;
                        this.total_correct_pixel_num++;
                    } else {
                        this.correct_data_dic[(r+1)+"-"+(c+1)] = false;
                        this.correct_data[r, c] = false;
                    }
                }
            }
        }

        /// <summary>
        /// CSVから出力した正解データを元に行列のヒント数字を作成する
        /// </summary>
        public int[,,] createHintValueMatrix()
        {
            // 行列の各ヒント数字を格納する配列を作成し、初期化する
            int[,] row_hint = new int[PlaySceneConst.SET_PIXEL_ROW_NUM,PlaySceneConst.SET_PIXEL_ROW_NUM];
            int[,] col_hint = new int[PlaySceneConst.SET_PIXEL_ROW_NUM,PlaySceneConst.SET_PIXEL_ROW_NUM];
            int[,,] hint_value = new int[2, PlaySceneConst.SET_PIXEL_ROW_NUM,PlaySceneConst.SET_PIXEL_ROW_NUM];
            for (int i = 0; i < PlaySceneConst.SET_PIXEL_ROW_NUM; i++) {
                for (int j = 0; j < PlaySceneConst.SET_PIXEL_ROW_NUM; j++) {
                    hint_value[0,i,j] = 0;
                    hint_value[1,i,j] = 0;
                }
            }

            // 行のヒント数字の算出
            int rh_point = 0;
            for (int r = 0; r < PlaySceneConst.SET_PIXEL_ROW_NUM; r++) {
                for (int c = 0; c < PlaySceneConst.SET_PIXEL_ROW_NUM; c++) {
                    if (true == this.correct_data[r, c]) {
                        // 正解だった場合はそのまま加算する
                        hint_value[0,r,rh_point]++;
                    } else {
                        // 不正解でかつ現在のポインタの値が1以上だった場合はポインタを進める
                        if (0 < hint_value[0,r,rh_point]) {
                            rh_point++;
                        }
                    }
                }
                rh_point = 0;
            }

            // 列のヒント数字の算出
            int ch_point = 0;
            for (int c = 0; c < PlaySceneConst.SET_PIXEL_ROW_NUM; c++) {
                for (int r = 0; r < PlaySceneConst.SET_PIXEL_ROW_NUM; r++) {
                    if (true == this.correct_data[r, c]) {
                        // 正解だった場合はそのまま加算する
                        hint_value[1,c,ch_point]++;
                    } else {
                        // 不正解でかつ現在のポインタの値が1以上だった場合はポインタを進める
                        if (0 < hint_value[1,c,ch_point]) {
                            ch_point++;
                        }
                    }
                }
                ch_point = 0;
            }

            return hint_value;
        }

        /// <summary>
        /// クリックされた座標にあるピクセル名を返却する
        /// </summary>
        /// <param name="target_position">クリックされた座標</param>
        public string clickedPixelName(Vector2 target_position)
        {
            // 子要素のピクセルを1個ずつ取得する
            foreach (KeyValuePair<string, GameObject> dic in this.pixel_children) {
                GameObject child = dic.Value;
                // ピクセル1個あたりの大きさを取得する
                float px_size = (child.GetComponent<RectTransform>().sizeDelta.x/2) * (child.GetComponent<RectTransform>().localScale.x);
                // クリックした座標がピクセル内にあるか判定する
                Vector2 px_position = Camera.main.WorldToScreenPoint(child.GetComponent<RectTransform>().position);
                if (px_position.x-px_size < target_position.x && target_position.x < px_position.x+px_size && px_position.y-px_size < target_position.y && target_position.y < px_position.y+px_size) {
                    return child.name;
                }
            }

            return null;
        }

        /// <summary>
        /// 対象のピクセル名のGameObjectが正解かどうか取得する
        /// </summary>
        /// <param name="target_name">対象のGameObject名</param>
        public bool isCorrectedPixel(string target_name)
        {
            // 存在しないGameObject名が入ってきた場合はfalseを返す
            if (false == this.correct_data_dic.ContainsKey(target_name)) {
                Debug.Log("Fatal");
                return false;
            }
            if (true == this.correct_data_dic[target_name]) {
                return true;
            } else {
                return false;
            }
        }
    }
}