# UI / Feedback 改修の検証記録

実施日: 2026-09-24 / Unity 2022.3.62f3 / Windows

- 全C#の静的コンパイル成功。コアチェック23件成功。
- 既存の通常投入・HOLD・最古順・残数保持のルールを維持。
- Unity Game Viewを1080×1920に固定してPlay Mode確認。
- Preview04操作シナリオを入力処理入口から実行し、4件完成、FLOW SCORE 550、BEST FLOW 450、ALL CLEAR 1、HOLD残数0を検証。
- CUP工程前のプレースホルダー、CUP後の空カップ、青→赤の水平積層、上部の白い泡を画像で確認。
- 3つのBUFFERアイコンと同時分岐移動、TRIPLE FLOW表示を画像で確認。
- 待ち時間切れの注文差し替えを、検証時のみ1秒で実行。
- RESULTと入力停止を、検証時のみ残り時間を1秒に短縮して確認。通常設定は60秒。
- ログ: POUR_CYCLE_PLAYMODE_PASS: Preview04, FLOW 550, best 450, stacked cups, CUP gating, expiry, result input lock

- Windows 64bitビルド成功: POUR_CYCLE_WINDOWS_BUILD_PASS。改修後EXEの起動・縦画面・カップ・HOLDアイコン表示を確認。

- Windows実行版で実時間60秒を待ってRESULTへ遷移、PLAY AGAINクリックで60秒・0点・初期注文に戻ることを確認。

## 未実施

Android/iOS実機センサー、実機フレームレート・音・爽快感の体感評価。Preview04の参考映像そのものは未提供のため、映像との完全一致は未確認です。



## Carousel / CLEAN改修

- 累積角度 +60 / -60、0.23秒 OutCubic。18回の通常SELECT（10回は約0.067秒間隔）、8回のBACKで目標角度と各フレームの単調性を検証。
- 1080×1920 Game ViewをUnity MediaEncoderで30fps無音MP4へ出力。
- 同解像度のDebugMode ON/OFF PNGを保存。撮影用CLEANで補正・利き手・キーボード説明・Tilt状態・Demo情報を非表示。
- Canvas Scaler: Scale With Screen Size / 1080×1920 / 幅基準。カップ1.35倍、ORDER材料・SELECTED・操作ボタン拡大。
- adb devicesの接続一覧は空。スマートフォン実機のPortrait撮影は未実施。Game View映像を実機映像とは扱っていません。

最終回帰検証: POUR_CYCLE_PLAYMODE_PASS（FLOW 550 / best 450、積層、CUP前提、Patience期限、RESULT入力停止）。更新後のWindows 64bitビルドもPOUR_CYCLE_WINDOWS_BUILD_PASS。

## UI / Carousel / Menu / Tilt 最終整理（2026-09-24）

- Main Menuを追加。起動直後はPLAY / PRACTICE / SETTINGSのみ表示。
- GameplayをORDER / 3 CUPS / 巨大Carouselの3ゾーンへ再配置。
- RIGHT HAND時はSELECT / POUR&ONを右下縦並び、LEFT HAND時は操作側UIのみ反転。
- Gameplay常設メニューは右上Pauseのみ。PauseはRESUME / TILT CALIBRATION / HOME。
- SettingsはHAND RIGHT/LEFTとTilt Calibrationへ整理。
- Resultは指定項目とPLAY AGAIN / HOMEのみ。
- FOAMは共通の雲状描画関数をORDER、Carousel、SELECTED、HOLD BUFFER、カップで使用。
- Carouselは累積角度方式を維持し、巨大円環へ拡大。
- 静的コンパイル成功。コアチェック23件成功。

未完了: この整理後のUnity Play Mode検証、キャプチャ生成、Windows再ビルド、実機Portrait撮影は、Unity Editorが「No valid Unity Editor license found」で起動できなかったため未実施。ライセンス復旧後に **POUR CYCLE > Verify Preview04 in Play Mode** と **POUR CYCLE > Capture carousel verification** を実行してください。

## Level Progression / Result / Game Tempo（2026-09-24）

- LevelConfigを追加。LEVEL 1-5のSessionDuration / OrderPatience / TargetFlow / Recipe Steps / 生成重みをGameControllerのLevels配列から調整可能。
- PLAYは常にLEVEL 1から開始。TITLEへ戻るとラン終了、永続保存とLEVEL SELECTはなし。
- LEVEL終了後はRESULTへ遷移。Target Flow以上ならLEVEL CLEAR、未達ならNOT CLEARED。
- CLEAR時はNEXT LEVEL / TITLE。未CLEAR時はTITLEのみ。LEVEL 5 CLEAR時はALL LEVELS CLEAR / TITLEのみ。
- PRACTICEはLEVEL進行から分離。時間制限なし、Target Flowなし、長めPatience。
- ORDER Patience切れはGAME OVERではなくMISSED / NEW ORDERとして注文単位で差し替え。
- 既存のSELECT、BACK、POUR&ON、HOLD、RELEASE ALL、Carousel、RIGHT/LEFT、Tilt Calibration、液体レイヤー、FLOW SCOREの入口は維持。
- 静的コンパイル成功。コアチェック27件成功。

LevelConfig初期値:

| Level | Duration | Patience | Recipe | Target Flow |
|---:|---:|---:|---|---:|
| 1 | 45s | 20s | 1-2 | 800 |
| 2 | 45s | 17s | 1-2 | 1200 |
| 3 | 45s | 14s | 2 | 1700 |
| 4 | 45s | 12s | 2-3 | 2300 |
| 5 | 45s | 10s | 2-3 | 3000 |

未完了: Unity Editorは今回も「No valid Unity Editor license found」で終了したため、Play Mode、Game Viewキャプチャ、Android実機スクリーンショット、Windows再ビルドは未実施。ライセンス復旧後に **POUR CYCLE > Capture carousel verification** を実行すると、LEVEL 1開始、LEVEL 1 CLEAR RESULT、NEXT LEVEL後のLEVEL 2開始、NOT CLEARED RESULT、LEVEL 5 COMPLETE、LevelConfig一覧が出力されます。
