// ==============================
// @author Nimanji (Indies a.k.a)
// ==============================

using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;

using Assets.Script.Const;
using Assets.Script.Model;
using Assets.Script.View;

// ==============================
// PuzzleScenePresenter
// ==============================
namespace Assets.Script
{
    /// <summary>
    /// PuzzleScenePresenter Class
    /// </summary>
    public class PuzzleScenePresenter : MonoBehaviour
    {
        // ViewとModelの生成
        private PuzzleSceneView view;
        private PuzzleSceneModel model;

        // 当たりのピクセルだった場合に返却されるGameObject名の受け取り
        private ReactiveProperty<string> correct_obejct_name = new ReactiveProperty<string>("");

        private void Awake()
        {
            // View, Modelインスタンスの生成
            this.view = new PuzzleSceneView();
            this.model = new PuzzleSceneModel();
            // ヒント数字の描画を行う
            this.view.drawHintValue(this.model.createHintValueMatrix());
        }
        
        private void Start()
        {
            // クリック時に実行するストリームの作成
            var click_stream = Observable.EveryUpdate().Where(_ => Input.GetMouseButtonDown(0));
            click_stream
                .Select(_ => Input.mousePosition)
                .Subscribe(world_position => {
                    string pixel_name = this.model.clickedPixelName(new Vector2(world_position.x, world_position.y));

                    if ("" != pixel_name) {
                        // ピクセル名を取得できた場合のみ正解/不正解の判定をする
                        bool is_correct = this.model.isCorrectedPixel(pixel_name);
                        if (true == is_correct) {
                            this.correct_obejct_name.Value = pixel_name;
                        } else {
                            this.correct_obejct_name.Value = PlaySceneConst.CORRECT_FAILED_TEXT;
                        }
                    }
                });

            // 正解のオブジェクトをクリックしたときのイベントを発行
            this.correct_obejct_name
                .Where(_ => "" != this.correct_obejct_name.Value)
                .Subscribe(_ => {
                    if (PlaySceneConst.CORRECT_FAILED_TEXT == this.correct_obejct_name.Value) {
                        SceneManager.LoadScene("PuzzleScene");
                        this.correct_obejct_name.Value = "";
                    } else {
                        this.correct_obejct_name.Value = this.view.changePixelModeByUnpushAndPush(this.correct_obejct_name.Value);
                    }
                });
        }
    }
}