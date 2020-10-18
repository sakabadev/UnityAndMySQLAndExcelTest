# UnityAndMySQLAndExcelTest

このプロジェクトを動かすためには以下のライブラリが必要になります。

Unity

- [MessagePack-CSharp](https://github.com/neuecc/MessagePack-CSharp) (v2)
- [MasterMemory](https://github.com/Cysharp/MasterMemory) (v2)
- MySql.Data.DLL と依存する DLL を Plugins フォルダに入れる
- NPOI.DLL と依存する DLL を Plugins フォルダに入れる

Excel Addin

- MySql.Data を NuGet で
- NPOI を NuGet で

Unity の Plugins に入れる DLL がわからない場合。  
VisualStudio 等で空のクラスライブラリ等のプロジェクトを作成し、参照に NuGet でダウンロードします。  
そうするとプロジェクトの packages フォルダに色々入ってきているので、各ライブラリの lib の中の良さげなバージョンの DLL を持ってきましょう。

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
