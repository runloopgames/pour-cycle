# POUR CYCLE™ — Vertical Slice

Unity **2022.3.62f3** 向けの独立プロジェクトです。既存の別ゲームは変更していません。添付の仕様文を基準に実装しました。PREVIEW / 04の動画・画像自体は今回の添付に含まれていないため、画面の完全一致は未確認です。

## 起動

**Unityなしですぐ試す場合:** 同梱の `PourCycleWindows/PourCycle.exe` を起動してください。フォルダー内のDLL・Dataフォルダーも必要です。Windows版は縦長ウィンドウで起動し、Left ShiftでTiltを代用できます。

1. Unity Hubの「Add project from disk」で、このREADMEがある `PourCycle` フォルダーを追加。
2. 2022.3.62f3で開き、パッケージのインポートを待つ。
3. 初回はシーンを自動生成します。生成されない場合はメニュー **POUR CYCLE > Build scene and verify** を実行。
4. `Assets/PourCycle/Scenes/PourCycle.unity` を開き、Gameビューを1080×1920 PortraitにしてPlay。

起動時はMain Menuです。PLAYは60秒セッション、PRACTICEは時間制限なし、SETTINGSではRIGHT / LEFTとTilt Calibrationを設定できます。初回パッケージ解決にはネット接続が必要な場合があります。有料アセットは不要です。

## 操作

| 操作 | 画面 | キーボード |
|---|---|---|
| 次の材料 | SELECT | A / 右矢印 |
| 前の材料 | Tilt + SELECT | Left Shift + A |
| 通常投入・RELEASE ALL | POUR&ON | Space / Enter |
| HOLD | Tilt + POUR&ON | Left Shift + Space |
| Pause | 右上の小さなII | Esc |
| Debug切替 | — | F1 |

ボタンを押し始めた瞬間のTiltだけを参照します。Tilt単独では注文やHOLDは変化しません。通常投入は該当する最古の注文1件のみ。HOLDは同じ材料を最大3個。別の材料はバッファーを残したまま通常投入できます。HOLDと同じ材料を通常姿勢で投入するとRELEASE ALLとなり、各注文に1個ずつ、古い順に使用します。余ったHOLDは残ります。

成功演出中は短時間入力をロックします。弧を描く材料移動0.25秒、注ぎ0.22秒、報酬表示0.38秒、退場・入場各0.15秒。ゲームプレイ中に常設するメニュー操作は右上のPauseだけです。

## UI / Feedback 改修

3つのORDERの下に製作中カップを配置。投入済みBLUE / RED / PINK / LIMEは水平な層として残り、FOAMは上部の白い泡になります。CUP工程がある注文は、CUPを処理するまでカップが現れません。

通常POURはSELECTED→カップ、HOLDはSELECTED→3スロット、RELEASE ALLはBUFFER→対象カップへ同時に弧を描いて移動します。保持数は小さな材料アイコンでも確認できます。

| 同じ操作で完成した注文 | 加点 | 表示 |
|---|---:|---|
| 1件 | 100 FLOW | +100 FLOW |
| 2件 | 250 FLOW | DOUBLE FLOW |
| 3件 | 450 FLOW | TRIPLE FLOW / ALL CLEAR |

BEST FLOWは1回の操作で得た最大FLOW、ALL CLEARは3件同時完成の回数です。途中工程には点数が入りません。同時に注いでも完成が1件なら100 FLOWです。

通常モードでは注文出現から15秒でPatienceが尽きます。時間切れの注文は途中の材料ごと差し替わり、得点は入りません。HOLDは残ります。演出中もほかの注文の時間は進み、差し替えは演出終了後に行います。Preview04は練習用に時計とPatienceを停止します。Freeze Patience In PreviewをOFFにすれば練習中のPatienceも動きます。

60秒経過したフレームで入力と演出を止め、RESULTへ移ります。制限時間前に受理した投入は、演出途中でも得点に含みます。RESULTではPLAY AGAINから60秒ゲームを再開します。ランキング・スコア保存・オンライン機能はありません。

## Preview04の手動テスト

初期状態は `FOAM / RED→FOAM / BLUE→LIME`、FOAM HOLD 1です。材料順は **FOAM → BLUE → RED → PINK → LIME → CUP**。

1. SELECT 2回でRED、POUR。ORDER 02がFOAMになる。
2. Shift + SELECT 1回でBLUE、Shiftを離してPOUR。ORDER 03がLIMEになる。
3. SELECT 3回でLIME、POUR。COMPLETE後にORDER 04 `BLUE→FOAM` が入る。
4. SELECT 2回でFOAM、Shift + POUR。HOLD 2。
5. SELECT 1回でBLUE、通常POUR。ORDER 04がFOAMになる。
6. Shift + SELECT 1回でFOAM、Shift + POUR。HOLD 3。
7. Shiftを離してPOUR。3方向へのRELEASE ALL、3件COMPLETE、ALL CLEAR。

続けて違う材料のHOLD拒否、4個目の拒否、対象が少ない場合の残数保持も確認してください。画面上部に処理件数、セッション終了時にORDERS CLEARED / BEST FLOW / ALL CLEAR / FLOW SCOREを表示します。

## Editor設定・テスト

Hierarchyの **POUR CYCLE** を選択します。

- GameController: Preview04 Mode / Session Seconds（初期60秒）/ Order Patience Seconds（初期15秒、10〜15秒を推奨）/ Freeze Patience In Preview（初期ON）。
- TiltInputController: Tilt Threshold（初期0.25）、Hysteresis（0.08）、Filter Speed（10）、Direction（0=両方向、1=正、-1=負）。
- **POUR CYCLE > Run core checks**: Preview04、最古1件、上限、混在拒否、残数、1注文1個、結果集計、タイマーなどを検証。
- **POUR CYCLE > Verify Preview04 in Play Mode**: 画面側のコントローラーを操作し、シナリオ・演出・注文差し替え・結果を自動検証。検証後はPreview04に戻ります。
- **POUR CYCLE > Build Windows player**: 隣の `PourCycleWindows` フォルダーにWindows版を出力します。
- `Tools/Verify.ps1`: Unity同梱コンパイラで全コードを静的コンパイルし、同じコア検証をMonoで実行。Editor Play Modeを代替するものではありません。

## 実機Tilt・レイアウトの確認

1. Build SettingsでAndroidまたはiOSへ切り替える。生成済みシーンをビルド対象にする。
2. Portrait設定を確認して端末にビルド。iOSの最終ビルド・署名はmacOS/Xcode側で行う。
3. 起動直後0.6秒は自然な通常姿勢を保つ。加速度の平均をNeutralに使用。
4. 横に傾けて **TILT ON** が出ること、傾けるだけではゲームが進まないことを確認。
5. 傾けたままSELECT / POURでBACK / HOLD、通常姿勢で通常投入・RELEASE ALLを確認。
6. 必要ならRECALIBRATE。復帰時にもNeutralを取り直す。
7. 左右の利き手設定、ノッチ端末、9:16・9:19.5・9:20の表示を確認。

UIは1080×1920の論理座標をSafe Area内に等比縮小し中央配置します。細長い端末では上下に余白を残します。RIGHT / LEFTは注文欄以外の円環・選択表示・操作ボタンを反転し、PlayerPrefsに保存します。

## ファイル構成

すべて新規作成です。

| ファイル | 責務 |
|---|---|
| Scripts/GameController.cs | 入力接続、UI組立、演出、モード切替 |
| Scripts/OrderManager.cs | Ingredient / OrderData、テンプレート注文生成、古い順の処理 |
| Scripts/HoldBufferController.cs | 同種3個までのバッファー |
| Scripts/PourController.cs | 通常投入・HOLD・RELEASE ALLの分岐 |
| Scripts/SessionController.cs | 制限時間・FLOW報酬・結果集計 |
| Scripts/OrderPatienceController.cs | 注文ごとの待ち時間 |
| Scripts/CupView.cs | カップ・積層液体・白い泡 |
| Scripts/TiltInputController.cs | Neutral、ローパス、閾値、ヒステリシス、Shift代替 |
| Scripts/HandednessController.cs | 左右反転と保存 |
| Scripts/CarouselController.cs | 材料選択と円環表示 |
| Scripts/SelectedItemView.cs | 選択材料・バッファー表示 |
| Scripts/OrderCardView.cs | レシピ・COMPLETE表示 |
| Scripts/UiFactory.cs | TMP・UI・生成スプライトの共通部品 |
| Scripts/PointerDownButton.cs | ボタンDOWN時の操作 |
| Scripts/AudioController.cs | 9種の合成効果音、HOLD音程変化 |
| Editor/PrototypeSetup.cs | シーン・Portrait・ビルド対象設定と検証メニュー |
| Editor/CoreChecks.cs | コアロジックの23チェック |
| Editor/PlayModeVerification.cs | ゲーム画面側のPreview04自動検証 |
| Tools/Verify.ps1 | Editorなしの静的コンパイル・ロジック検証 |
| Packages/manifest.json | TMP・uGUI・必要なUnityモジュール |
| ProjectSettings/ProjectVersion.txt | Unityバージョン指定 |

上記のScripts / Editorは `Assets/PourCycle/` 配下です。生成したシーン、.meta、各種ProjectSettings、TMP Essential Resources（付属フォント・ライセンス含む）も同梱しています。

## 現在の検証状況と課題

- 全C#コードの静的コンパイル成功、コア23チェック成功。
- Unity Editorコンパイル成功。Play ModeのPreview04自動検証成功。Windows 64bitビルド・起動・左右切替を確認済み。
- Unity Editorでの検証状況は `VERIFICATION.md` に記載。
- 実機センサーの閾値調整、iOS/Androidでの表示と音の体感評価は未実施。
- PREVIEW / 04の実際の参考映像との照合は未実施。
- レシピは8テンプレート、1〜3工程。FOAMで揃える機会を多めにした初期バランスです。通常モードの最初の3件も混在した固定盤面で、その後はランダム生成します。
- 失敗時は状態不変、音と揺れのみ。後でPourResultの失敗にタイムペナルティを接続できます。
- BGM、オンライン機能、広告、課金、分析SDKは含みません。


## Product UI / Carousel（2026-09-24）

起動時はMain Menuで、商品画面にはPLAY / PRACTICE / SETTINGSだけを表示します。Gameplay中は上部ORDER、中央3 CUPS、下部巨大Carouselの3ゾーン構成です。F1でCLEAN / DEBUGを切り替えます。スマートフォンではビルド前にInspectorのDebug Modeを指定できます。

円環はCurrentRotation / TargetRotationを累積し、SELECTで+60度、BACKで-60度。indexの5→0でも角度を戻しません。初期0.23秒のOutCubicで、Select DurationはInspectorから0.18〜0.28秒に調整できます。回転中のSELECTも目標へ加算します。アイコンは累積角度から円周上の位置のみを更新し、絵・文字は正立します。

Canvas ScalerはScale With Screen Size、1080×1920、幅基準。Safe Area内で等比配置し、Carouselは約820pxの円環として左下へ大きく配置します。3 CUPSは中央で1.45倍、SELECTEDは右側に大きく配置し、RIGHT HAND時は右下にSELECT / POUR&ONを縦並びにします。LEFT HAND時は操作側UIだけを左右反転します。CLEANでは操作ガイド・Tilt状態・補正・利き手・Demo情報を非表示にします。

PauseはRESUME / TILT CALIBRATION / HOMEのみです。FOAMは共通の雲状Sprite描画をORDER、Carousel、SELECTED、HOLD BUFFER、完成カップで使い回します。

POUR CYCLE > Capture carousel verification は、1080×1920 Game ViewからMain Menu、RIGHT HAND、LEFT HAND、Debug、液体層、FOAM完成、Result、SELECT動画、BACK動画を出力します。SELECTの入力処理入口を通常18回（高速連打10回含む）、BACK8回呼び、単調な回転と入力数を確認します。実際の指・センサー入力を撮影したものではありません。

## Level Progression（2026-09-24）

PLAYは必ずLEVEL 1から開始します。1LEVELは初期45秒で、終了後はRESULTへ移動します。Target Flow以上ならLEVEL CLEAR、LEVEL 5ならALL LEVELS CLEARです。CLEAR時だけNEXT LEVELを表示し、TITLEへ戻ると現在のランは終了します。次回PLAYは常にLEVEL 1です。未CLEAR時はTITLEのみ表示します。

PRACTICEはLEVEL進行から独立しています。時間制限、Target Flow、NEXT LEVELはなく、長めのPatienceでTilt / HOLD / RELEASE ALLを試せます。

LevelConfigはGameControllerの`Levels`配列で調整できます。

| Level | Duration | Patience | Recipe | Target Flow |
|---:|---:|---:|---|---:|
| 1 | 45s | 20s | 1-2 steps | 800 |
| 2 | 45s | 17s | 1-2 steps | 1200 |
| 3 | 45s | 14s | 2 steps | 1700 |
| 4 | 45s | 12s | 2-3 steps | 2300 |
| 5 | 45s | 10s | 2-3 steps | 3000 |

POUR CYCLE > Capture carousel verification はLEVEL 1開始、LEVEL 1 CLEAR RESULT、LEVEL 2開始、NOT CLEARED RESULT、LEVEL 5 COMPLETE、Practice、LevelConfig一覧も出力します。

