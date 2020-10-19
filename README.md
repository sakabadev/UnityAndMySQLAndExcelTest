# UnityAndMySQLAndExcelTest

Unity と Excel で MySql を介してデータの編集を行います。

「UntiyEditor と Excel、両方ともいい感じに使ってマスターデータの設定を行いたい、規模的には個人で作れる範囲で」  
の気持ちを元に出来ています。

CEDEC2019 の FF14 の NEX の発想を参考にしています。  
しかし、同期したり差分を取ったり串刺し Lookup したり出来るものではなく、あくまで上記の事がそこそこ楽に出来るようになればいいなぁ程度の目的ですので悪しからず。

[簡単な紹介動画はこちら](https://youtu.be/2JuSoOIIYX0)

## 必要な知識

- MessagePack-CSharp v2 と MasterMemory v2 の最低限な使い方
- Unity の Editor 拡張について
- MySql を個人で運用するための知識（MAMP 等でも可。立ち上げて接続していじれれば OK）

## Setup

このプロジェクトを動かすためには以下のライブラリが必要です。

== Unity ==

- [MessagePack-CSharp](https://github.com/neuecc/MessagePack-CSharp) (v2)
- [MasterMemory](https://github.com/Cysharp/MasterMemory) (v2)
- MySql.Data.DLL と依存する DLL を Plugins フォルダに入れる
- NPOI.DLL と依存する DLL を Plugins フォルダに入れる

Unity の Plugins に入れる DLL がわからない場合。  
VisualStudio 等で空のクラスライブラリ等のプロジェクトを作成し、参照に NuGet でダウンロードします。  
そうするとプロジェクトの packages フォルダに色々入ってきているので、各ライブラリの lib の中の良さげなバージョンの DLL を持ってきましょう。

== Excel Addin ==

- MySql.Data を NuGet で
- NPOI を NuGet で

また MD2DBFromExcel がメインプロジェクトであり、

- MD2DBFromExcel => Infrastructure と Domain に依存
- MD2DBFromExcel.Infrastructure => Domain に依存
- MD2DBFromExcel.Domain => 依存なし

となっているため、参照が切れている場合はプロジェクトに参照を追加する必要があります。

## 使い方

簡単な使用の流れですが、

### [Unity]

① Unity と /ExcelAddin/MD2DBFromExcel.Infrastructure の中の MySQLHelper.cs に MySql への接続情報を記述する。

② MemoryTable 属性のある Entity を作り、コードジェネレートする。既存の Entity の属性を参考に属性を書くことと、ついでに値編集用の EditorWindow も作ります。（既に作ってある Item クラスを元に説明します。）

③ UnityEditor 上部の Sakaba/Open ExcelFileCreatorWindow を開き、"update MemoryTable list"を押す。

④ プルダウンでテーブルを作りたいクラスを選び、"create xlsx"と"create table to MySql"を押す。

⑤ /Excels/に xlsx が生成され、DB に対象のクラス用テーブル（class_name テーブル、class_name_config テーブル）が作れた事を確認する。

### [Excel]

⑥ /ExcelAddin/MD2DBFromExcel/MD2DBFromExcel.sln をビルドし、Excel の VSTO アドインをインストールする。

⑦ xlsx を開き、値を編集したいシートを選択してリボンの sakaba > "選択中のシートを読み込む"をクリックする。

⑧ 少し待つと上部には列の設定データが入力され、その下にテーブルが作られるので、id を定義していく。ついでに入力したいフィールドがあれば入力する。

⑨ リボンから”選択中のシートを保存”をクリックする。成功すると、MySql にテーブルへ入力した情報が保存される。

### [Unity]

⑩ UnityEditor に戻り、Sakaba/DBEditor/Item をクリック、マスターデータ Editor を開く。

⑪ 左上の Import を押すと、MySql のデータが MemoryDatabase に保存される。具体的に言うと Assets/Sakaba/Resources/MB.bytes に保存される。

⑫ いい感じに編集し、EditorWindow 左下の Save をクリックすると、Assets/Sakaba/Resources/MB.bytes に保存される。

⑬ Assets/Sakaba/Resources/MB.bytes に保存した状態で Export ボタンを押すと、MySql に保存される。

大まかな流れは以上です。

## config テーブル

コンフィグの値を見て、Excel シートに色々と行っています。

- prefer_excel の値を 1 にすると、Excel へ Import した時に対象の列は上書きしません。null なら書き込みます。  
  これにより、Excel の表の中で関数を使えるようにしています。
- column_width は Import 時にセルの幅をいじります。
- column_name は書き込むフィールドカラムを指定するためのものです。
- sort_label は表の見出しになります。

## 注意点

- Excel ファイルの 1 列目の"key","value"をずらしたり消したりすると正常に処理ができなくなります。
- 関数が入っているセルから値を取得する処理が未実装なため、まだ関数は使えません。

## License

====================

The MIT License (MIT)

Copyright (c) 2020 sakabadev

Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the "Software"), to deal in
the Software without restriction, including without limitation the rights to
use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
the Software, and to permit persons to whom the Software is furnished to do so,
subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
