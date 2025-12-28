# ONE MORE ROULETTE
unity1week「もうひとつ」用開発プロジェクト

## アーキテクチャ概要（MVP）
- **Model**: `RouletteModel`。UnityEngine へ依存しない純粋な C# で、弾数・ランク・報酬ロール・スコア計算などのゲームロジックを担当。
- **Presenter**: `RunController`。ステートマシンを持ち、Model を操作し、`GameViewModel` を更新して `IGameView` のアニメーションを順番に await します。
- **ViewModel**: `GameViewModel`（R3）。UI は読み取り専用で、書き込みは Presenter のみ。
- **View**: `GameView`（`IGameView` を実装する MonoBehaviour）と `UiBinder`。ボタン入力やテキスト更新を束ね、ロジックは持ちません。

## 最低限のシーンセットアップ
詳細な手順は `SETUP.md` を参照してください。概要は以下の通りです。
1. 空の GameObject に **RunController** をアタッチし、`_config` に **GameConfig**、`_view` に **GameView** 実装、`_uiBinder` に **UiBinder** を割り当てます。
2. UI 側の GameObject に **UiBinder** を追加し、ボタンとテキストの参照をシリアライズフィールドに設定します。
3. `Create > OneMoreRoulette > GameConfig` から設定アセットを作成し、必要なら報酬テーブルや倍率を調整します。
4. Play を押せば、Decision ステート中のみ入力を受け付ける形で全 5 ラウンドのループが自動で実行されます。

アニメーション用のフックはすべて `IGameView`（UniTask 戻り値）に定義済みです。DOTween/TMP などの演出は `GameView` 継承クラスで自由に実装してください（ゲームロジックへ影響なし）。
