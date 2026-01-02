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
`RunController`・`GameView`・`UiBinder` の 3 コンポーネントがそろえばゲームループが動きます。以下のような最小ヒエラルキーを推奨します。

```
GameRoot (Empty)
├── RunController (Component)
├── GameView (Component)
└── UiCanvas (Canvas)
    └── UiBinder (Component)
        ├── Buttons ... OneMoreButton / StopButton
        └── Texts   ... Round / Dead / Bullet / Rank / Multiplier / CarryNext / RoundScore / TotalScore
```

1. **GameRoot のセットアップ**
   - 空の GameObject を `GameRoot` とし、`RunController` を追加。演出がシンプルなら同じオブジェクトに `GameView` も追加しておくと参照がずれません。
   - `RunController._config` に作成済みの **GameConfig** を drag & drop。
   - `RunController._view` には同一オブジェクト上の `GameView` を drag & drop。
   - `RunController._uiBinder` には後述の `UiCanvas` 配下の `UiBinder` を設定。

2. **UI（UiCanvas）のセットアップ**
   - Canvas を `UiCanvas` として作成（推奨: Screen Space - Overlay）。イベント受信用に `Graphic Raycaster` と `EventSystem` があることを確認。
   - `UiCanvas` に `UiBinder` をアタッチ。
   - ボタン: `_oneMoreButton`, `_stopButton` に Unity UI Button コンポーネントを割り当てる（Decision ステート時のみクリックが反映）。
   - テキスト: `_roundText`, `_deadText`, `_bulletText`, `_rankText`, `_multiplierText`, `_carryNextText`, `_roundScoreText`, `_totalScoreText` にそれぞれの Text/ TMP_Text を接続。
   - UiCanvas の位置・アンカーは任意で OK。RunController とはシリアライズ参照でつながるだけなので、ヒエラルキー上は子でなくても動作します（整理のために子に置く例を上記に記載）。

3. **GameView をカスタムする場合**
   - `GameView` を継承したクラスを作り、`RunController._view` にそのコンポーネントを指定。
   - アニメーション対象（リールの Transform、パーティクル、DOTween 用シーケンスなど）への参照フィールドを用意し、同じオブジェクトか子オブジェクトでシリアライズしておくと見失いにくいです。
   - **演出のトリガー例:**
     - `OnStateChangedAsync(GameViewModel.State state)`: ステート遷移時にまとめて演出を切り替える。例: Decision → Fire で発砲アニメ、GameOver → Result でリザルト画面表示。
     - `OnRoundUpdated(GameViewModel vm)`: ラウンド更新時にランク、弾数、倍率の表示をアニメ付きで更新する。
     - `OnResultUpdated(GameViewModel vm)`: スコア確定時にリザルトパネルを出し、DOTween でスコア増減を演出。
   - **カスタム クラスの作り方のコツ:**
     - 画面ごとに MonoBehaviour を分けたい場合、`MyGameView : GameView` を UiCanvas とは別の GameObject に置き、`RunController._view` に指定。UiBinder とは参照で接続されるため物理的に分離しても問題ありません。
     - DOTween の Sequence を事前に組み立てておき、`OnStateChangedAsync` の中で `Play`/`Restart` する形にすると、フローと演出を同期しやすくなります。
     - 大きなステート遷移演出（GameOver など）と軽微な数値更新演出を別メソッドに分け、`OnStateChangedAsync` と `OnRoundUpdated` から呼び分けると保守しやすいです。
     - 非同期演出を追加する場合も、`GameView` 基底の公開メソッドシグネチャ（`OnStateChangedAsync` など）は変えず、内部で `async/await` を活用してください。

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
