# セットアップと動作確認手順（日本語）

このドキュメントは **ONE MORE ROULETTE** のスクリプト導入手順をまとめたものです。

## 必須スクリプト一覧
- `Assets/Scripts/Config/GameConfig.cs` : ルール・報酬テーブルを保持する ScriptableObject。
- `Assets/Scripts/Model/RouletteModel.cs` : ゲームロジック本体。UnityEngine 非依存。
- `Assets/Scripts/Core/GameStateMachine.cs` : 簡易ステートマシン。
- `Assets/Scripts/UI/GameViewModel.cs` : R3 のリアクティブ値。
- `Assets/Scripts/UI/GameView.cs` : アニメーション用フック（DOTween などを後付け）。
- `Assets/Scripts/UI/UiBinder.cs` : UI へのバインドとボタン入力。
- `Assets/Scripts/UI/RunController.cs` : プレゼンター。フロー制御と Model との橋渡し。

これらでゲームループが成立します。UI 演出が未実装でもプレイモードでループ確認が可能です。

## GameConfig アセット作成
1. Project ウィンドウで `Create > OneMoreRoulette > GameConfig` を選択。
2. インスペクターで以下を必要に応じて調整:
   - `Max Rounds` / `Dead Limit` / `Cylinder Size` / `Carry Rate` / `Rank Cap`
   - `Rank To Multiplier`（11 要素）
   - `Reward Bands By K`（k=1..5 の報酬エントリ）

## シーンへの配置
1. 空の GameObject を作成し `RunController` をアタッチ。
   - `_config` に作成した **GameConfig** を設定。
   - `_view` に `GameView`（または継承クラス）を設定。
   - `_uiBinder` に `UiBinder` を設定。
2. UI 用 GameObject に `UiBinder` をアタッチし、以下をシリアライズフィールドに割り当て:
   - `_oneMoreButton`, `_stopButton`（クリックは Decision ステート時のみ通過）
   - テキスト: `_roundText`, `_deadText`, `_bulletText`, `_rankText`, `_multiplierText`, `_carryNextText`, `_roundScoreText`, `_totalScoreText`
3. 必要なら `GameView` を継承してアニメーションを実装し、`RunController._view` に差し替え。

## 再生と確認ポイント
- 再生開始で自動的に 1 ラウンド目が始まります。
- `ONE MORE` ボタン: ランク上昇→弾装填→発砲→SAFE なら報酬計算、DEAD ならラウンド終了。
- `STOP` ボタン: 現在のラウンドスコアを確定し、ランクを半分（切り捨て）持ち越して次ラウンドへ。
- `DeadCount` が 2 に達すると即座に GameOver へ遷移します。
- ステートは `GameViewModel.State` から確認できます（Decision 以外は入力無効）。

## デバッグ用のヒント
- `RunController.StartRunAsync(int? seed = null)` に任意の seed を渡すと報酬/発砲結果を固定できます。
- Model 側は純粋 C# なので、エディタ外で単体テストを書く際にも参照できます（UnityEngine 非依存）。

## 既知の未実装
- `GameView` の演出はスタブです。DOTween などで自由に拡張してください。
- サウンド・リーダーボード連携は含まれていません。
