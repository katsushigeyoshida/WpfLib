using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WpfLib
{
    public enum BUTTONTYPE { RECT, CIRCLE, GROUPCIRCLE, GROUPRECT };

    /// <summary>
    /// グループボタンクラス
    /// </summary>
    public class GroupGButton
    {
        int mId;
        BUTTONTYPE mType;
        Rect mRect;         //  全体の大きさ
        int mRowCount;      //  行数
        int mColCount;      //  列数　これが0の時は円形配置

        /// <summary>
        /// コンストラクタ グループボタンの設定(まとめてボタンを作成する)
        /// 列数が0の時は円形配列とし、それ以外は矩形に配列する
        /// </summary>
        /// <param name="id">基準ID</param>
        /// <param name="type">種類(GROUPCIRCLE/GROUPRECT)</param>
        /// <param name="rect">全体の領域</param>
        /// <param name="row">行数</param>
        /// <param name="col">列数</param>
        public GroupGButton(int id, BUTTONTYPE type, Rect rect, int row, int col)
        {
            mId = id;
            mType = type;
            mRect = rect;
            mRowCount = row;
            mColCount = col;
        }

        /// <summary>
        /// ボタンの数
        /// </summary>
        /// <returns></returns>
        public int getSize()
        {
            if (mColCount == 0)
                return mRowCount;
            else
                return mRowCount * mColCount;
        }

        /// <summary>
        /// グラフィックボタンデータの作成
        /// </summary>
        /// <returns>GButtonの配列</returns>
        public GButton[] getButton()
        {
            if (mType == BUTTONTYPE.GROUPCIRCLE) {
                return getButtons(BUTTONTYPE.CIRCLE);
            } else if (mType == BUTTONTYPE.GROUPRECT) {
                return getButtons(BUTTONTYPE.RECT);
            }
            return null;
        }

        /// <summary>
        /// 円ボタンの一括データ作成
        /// </summary>
        /// <returns>GButtonの配列</returns>
        private GButton[] getButtons(BUTTONTYPE buttonType)
        {
            GButton[] gbuttons;
            if (0 < mRowCount && mColCount == 0) {
                //  円形配置
                gbuttons = new GButton[mRowCount];
                double or = mRect.Width / 2;
                double ratio = mRect.Height / mRect.Width;
                Point oc = new Point(mRect.X + or, mRect.Y + or * ratio);
                double ir = or * Math.Sin(Math.PI / mRowCount) / (Math.Sin(Math.PI / mRowCount) + 1);
                double cr = or - ir;
                double dth = 2 * Math.PI / mRowCount;
                int i = 0;
                for (double th = 0; th < 2 * Math.PI && i < mRowCount; th += dth) {
                    double cx = oc.X + cr * Math.Cos(th);
                    double cy = oc.Y + cr * Math.Sin(th) * ratio;
                    Rect inrect = new Rect(new Point(cx - ir, cy - ir * ratio), new Point(cx + ir, cy + ir + ratio));
                    gbuttons[i++] = new GButton(mId + i, buttonType, inrect);
                }
            } else if (0 < mRowCount && 0 < mColCount) {
                //  矩形配置
                gbuttons = new GButton[mRowCount * mColCount];
                double width = mRect.Width / mColCount;
                double height = mRect.Height / mRowCount;
                double sx = mRect.Left;
                double sy = mRect.Top;
                int i = 0;
                for (int row = 0; row < mRowCount; row++) {
                    for (int col = 0; col < mColCount; col++) {
                        Rect rect = new Rect(sx + width * col, sy + height * row, width, height);
                        //new Point(sx + width * col, sy - height * row),
                        //new Point(sx + width * (col + 1), sy - height * (row + 1)));
                        gbuttons[i++] = new GButton(mId + i, buttonType, rect);
                    }
                }
            } else {
                return null;
            }
            return gbuttons;
        }

        /// <summary>
        /// 矩形ボタンの一括データ作成
        /// </summary>
        /// <returns>GButtonの配列</returns>
        private GButton[] getRectButton()
        {
            return null;
        }
    }

    /// <summary>
    /// WFP グラフィックライブラリ
    ///     グラフィックボタンをワールド座標で表示するため、ワールド座標の設定が必要
    /// YGButton → YWorldShapes → YDrawingShapes
    /// 
    /// YWorldShapes の上にグラフィックボタンを追加
    /// 
    /// YGButton()                              コンストラクタ
    /// YGButton(Canvas c)                      コンストラクタ
    /// GButtonClear()                          グラフィックボタンデータをクリア
    /// GButtonAdd(int id, BUTTONTYPE type, Rect rect)  グラフィックボタンの追加
    /// GButtonGroupAdd(int id, BUTTONTYPE type, Rect rect, int rowCount, int colCount, string[] titles)    複数ボタン一括追加
    /// GButtonResize(int id, BUTTONTYPE type, Rect rect)   ラフィックボタンの大きさと位置を変更
    /// GButtonRemove(int id)                   グラフィックボタンの削除
    /// GButtonDraws()                          グラフィックボタンをすべて表示
    /// GButtonBorderColr(int id, Brush color)  ボタン枠の色設定
    /// GButtonBorderThickness(int id, double thickness)    ボタン枠の太さの設定
    /// GButtonBackColr(int id, Brush color)    ボタンの背景色の設定
    /// GButtonTitle(int id, string title)      ボタンのタイトル設定
    /// GButtonTitleAlignment(int id, HorizontalAlignment ha, VerticalAlignment va) ボタンタイトルのアライメントの設定
    /// GButtonTitleRatio(int id, double ratio) ボタンの大きさに対するタイトル文字の大きさの比率
    /// GButtonTitleColor(int id, Brush color)  ボタンのタイトル文字の色設定
    /// GButtonTitleIdGet(string title)         タイトル文字からボタンのIDを検索する
    /// GButtonResource(int id, System.Drawing.Bitmap bitmap)   リソースファイルを設定
    /// GButtonImageFile(int id, string filePath)   イメージファイルを設定
    /// GButtonDownId(Point pt)                 押されたボタンのIDを取得
    /// GButtonDownSet(int id, bool down)       ボタンの状態(押下:down)の設定
    /// GButtonDownGet(int id)                  ボタンの状態を取得
    /// GButtonDwonReversId(int id)             ボタンの状態(押した)を反転
    /// GButtonDownClear()                      全ボタンの押した状態をクリアする
    /// GButtonDownCount()                      押されているボタンの数
    /// 
    /// 個別ボタンの設定
    /// プロパティ
    ///     mId                     ボタンのID
    ///     mType                   ボタンの種類
    ///     mRect                   ボタンの大きさ
    ///     mBorderColor            枠の色
    ///     mBorderThickness        枠の幅
    ///     mTitle                  タイトル
    ///     mTitleSize              タイトル文字の大きさ
    ///     mTitleColor             タイトル文字の色
    ///     mTitleLocation          タイトル文字の起点相対座標(未使用)
    ///     mTitleRatio             タイトル文字サイズ比
    ///     mHorizontalAlignment    水平文字配置
    ///     mVerticalAlignment      垂直文字配置
    ///     mBackColor              背景色
    ///     mDownBackColor          ボタンを押した時の背景色
    ///     mDown                   ボタンを押された状態
    ///     mResouceFile            リソースファイル
    ///     mImageFile              イメージファイルのパス
    /// 
    /// GButton(int id, BUTTONTYPE type, Rect rect)     ボタンの登録
    /// setTitle(string title, double measureTextRatio) ボタンタイトルの設定
    /// getButtonIn(Point p, Box world)                 ボタンの内外判定
    ///  
    /// 
    /// </summary>

    public class YGButton : YWorldShapes
    {
        Dictionary<int, GButton> mGButton = null;           //  表示図形リスト
        Dictionary<int, GroupGButton> mGButtonGroup = null; //  グループデータのIDと数

        public YGButton()
        {

        }

        public YGButton(Canvas c)
        {
            mCanvas = c;
        }

        /// <summary>
        /// グラフィックボタンデータをクリア
        /// </summary>
        public void GButtonClear()
        {
            if (mGButton != null)
                mGButton.Clear();
        }

        /// <summary>
        /// グラフィックボタンの追加
        /// </summary>
        /// <param name="id">ID</param>
        /// <param name="type">ボタンの種類</param>
        /// <param name="rect">位置と大きさ</param>
        public void GButtonAdd(int id, BUTTONTYPE type, Rect rect)
        {
            if (mGButton == null)
                mGButton = new Dictionary<int, GButton>();
            GButton button = new GButton(id, type, rect);
            //  既に存在する場合は削除してから追加
            if (mGButton.ContainsKey(id))
                mGButton.Remove(id);
            mGButton.Add(id, button);
        }

        /// <summary>
        /// グラフィックボタンを複数一括登録
        /// ボタンを同一の大きさで行列単位で配置する
        /// 列指定が0の時は円形に配置する
        /// </summary>
        /// <param name="id">基準ID</param>
        /// <param name="type">ボタンの種類</param>
        /// <param name="rect">配置するボタンの領域</param>
        /// <param name="rowCount">行数</param>
        /// <param name="colCount">列数</param>
        /// <param name="titles">ボタンの文字配列データ</param>
        public void GButtonGroupAdd(int id, BUTTONTYPE type, Rect rect, int rowCount, int colCount, string[] titles)
        {
            if (type != BUTTONTYPE.GROUPCIRCLE && type != BUTTONTYPE.GROUPRECT)
                return;
            if (mGButton == null)
                mGButton = new Dictionary<int, GButton>();
            if (mGButtonGroup == null)
                mGButtonGroup = new Dictionary<int, GroupGButton>();

            if (mGButtonGroup.ContainsKey(id))
                mGButtonGroup.Remove(id);
            GroupGButton groupGButton = new GroupGButton(id, type, rect, rowCount, colCount);
            mGButtonGroup.Add(id, groupGButton);
            GButton[] gbuttons = groupGButton.getButton();
            //  既に存在する場合は削除してから追加
            for (int i = 0; i < gbuttons.Length; i++) {
                if (mGButton.ContainsKey(id + i))
                    mGButton.Remove(id + i);
                mGButton.Add(id + i, gbuttons[i]);
                if (i < titles.Length)
                    GButtonTitle(id + i, titles[i]);
            }
        }

        /// <summary>
        /// IDが登録されているか確認
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool GButtonContainsKey(int id)
        {
            return mGButtonGroup.ContainsKey(id);
        }

        /// <summary>
        /// グラフィックボタンの大きさと位置を変更する
        /// 元々ない場合には新規作成する
        /// </summary>
        /// <param name="id">ID</param>
        /// <param name="type">ボタンの種類</param>
        /// <param name="rect">位置と大きさ</param>
        public void GButtonResize(int id, BUTTONTYPE type, Rect rect)
        {
            if (mGButton == null)
                mGButton = new Dictionary<int, GButton>();
            //  既に存在する場合は削除してから追加
            if (mGButton.ContainsKey(id)) {
                GButton button = mGButton[id];
                button.mType = type;
                button.mRect = rect;
                mGButton[id] = button;
            } else {
                GButton button = new GButton(id, type, rect);
                mGButton.Add(id, button);
            }
        }

        /// <summary>
        /// グラフィックボタンの削除
        /// </summary>
        /// <param name="id">ID</param>
        public void GButtonRemove(int id)
        {
            if (mGButton == null)
                return;
            if (mGButtonGroup != null && mGButtonGroup.ContainsKey(id)) {
                for (int i = 0; i < mGButtonGroup[id].getSize(); i++) {
                    if (mGButton.ContainsKey(id + i))
                        mGButton.Remove(id + i);
                }
            } else {
                if (mGButton.ContainsKey(id))
                    mGButton.Remove(id);
            }
        }

        /// <summary>
        /// 画像ファイルでトリミングする領域を指定する
        /// </summary>
        /// <param name="id">ID</param>
        /// <param name="rect">トリミングサイズ</param>
        /// <param name="size">もとの画像サイズ</param>
        public void GButtonTrimmingSize(int id, Rect rect, Size size)
        {
            if (mGButton == null)
                return;
            if (mGButton.ContainsKey(id)) {
                mGButton[id].mTrimmingRect = rect;
                mGButton[id].mImageSize = size;

            }
        }

        /// <summary>
        /// グラフィックボタンをすべて表示
        /// </summary>
        public void GButtonDraws()
        {
            if (mGButton == null)
                return;
            foreach (KeyValuePair<int, GButton> kvp in mGButton) {
                draw(kvp.Value);
            }
        }

        /// <summary>
        /// グラフィックボタンの表示
        /// </summary>
        /// <param name="button">グラフィックボタンデータ</param>
        private void draw(GButton button)
        {
            //  表示/非表示確認
            if (!button.mVisible)
                return;
            //  ボタン枠の表示
            setColor(button.mBorderColor);
            setThickness(button.mBorderThickness);
            setFillColor(button.mDown ? button.mDownBackColor : button.mBackColor);
            draw(button.mType, button.mRect);

            //  リソースファイルまたはイメージファイルを表示
            if (button.mResouceFile != null) {
                drawBitmap(button.mResouceFile, button.mRect);
            } else if (button.mImageFile != null) {
                if (button.mTrimmingRect == null || 
                    button.mTrimmingRect.Width == 0 || button.mTrimmingRect.Height == 0)
                    drawImageFile(button.mImageFile, button.mRect);
                else
                    drawImageFile(button.mImageFile, button.mRect, button.mTrimmingRect, button.mImageSize);
            }

            //  ボタンタイトルの表示
            if (0 < button.mTitle.Length) {
                setTextColor(button.mTitleColor);
                setTextSize(button.mTitleSize * (button.mType == BUTTONTYPE.CIRCLE?button.mTitleCircleRatio:1));
                double x = button.mRect.Left;
                double y = button.mRect.Top;
                if (button.mHorizontalAlignment == HorizontalAlignment.Center)
                    x += Math.Sign(mWorld.Right - mWorld.Left) * button.mRect.Width / 2;
                if (button.mHorizontalAlignment == HorizontalAlignment.Right)
                    x += Math.Sign(mWorld.Right - mWorld.Left) * button.mRect.Width;
                if (button.mVerticalAlignment == VerticalAlignment.Center)
                    y += Math.Sign(mWorld.Bottom - mWorld.Top) * button.mRect.Height / 2;
                if (button.mVerticalAlignment == VerticalAlignment.Bottom)
                    y += Math.Sign(mWorld.Bottom - mWorld.Top) * button.mRect.Height;
                drawText(button.mTitle, new Point(x, y), 0, button.mHorizontalAlignment, button.mVerticalAlignment);
            }
        }

        /// <summary>
        /// ボタン枠の表示
        /// </summary>
        /// <param name="type">ボタンの種類</param>
        /// <param name="rect">位置と大きさ</param>
        private void draw(BUTTONTYPE type, Rect rect)
        {
            if (type == BUTTONTYPE.RECT) {
                drawRectangle(rect, 0);
            } else if (type == BUTTONTYPE.CIRCLE) {
                drawOval(rect, 0);
            }
        }

        /// <summary>
        /// ボタン枠の色設定
        /// </summary>
        /// <param name="id">ID</param>
        /// <param name="color">色</param>
        public void GButtonBorderColr(int id, Brush color)
        {
            if (mGButton == null)
                return;
            if (mGButton.ContainsKey(id))
                mGButton[id].mBorderColor = color;
        }

        /// <summary>
        /// ボタン枠の太さの設定
        /// </summary>
        /// <param name="id">ID</param>
        /// <param name="thickness">太さ</param>
        public void GButtonBorderThickness(int id, double thickness)
        {
            if (mGButton == null)
                return;
            if (mGButton.ContainsKey(id))
                mGButton[id].mBorderThickness = thickness;
        }

        /// <summary>
        /// ボタンの背景色の設定
        /// 背景を塗潰さない時(透明)は null を指定する
        /// </summary>
        /// <param name="id">ID</param>
        /// <param name="color">色</param>
        public void GButtonBackColor(int id, Brush color)
        {
            if (mGButton == null)
                return;
            if (mGButton.ContainsKey(id))
                mGButton[id].mBackColor = color;
        }

        /// <summary>
        /// ボタンのタイトル設定
        /// </summary>
        /// <param name="id">ID</param>
        /// <param name="title">タイトル文字列</param>
        public void GButtonTitle(int id, string title)
        {
            if (mGButton == null)
                return;
            if (mGButton.ContainsKey(id)) {
                if (0 < title.Length)
                    mGButton[id].setTitle(title, measureTextRatio(title));
                else
                    mGButton[id].mTitle = "";
            }
        }

        /// <summary>
        /// リソースファイルを設定する
        /// </summary>
        /// <param name="id">ID</param>
        /// <param name="bitmap">リソースファイル</param>
        public void GButtonResource(int id, System.Drawing.Bitmap bitmap)
        {
            if (mGButton == null)
                return;
            if (mGButton.ContainsKey(id)) {
                mGButton[id].setResourceFile(bitmap);
            }
        }

        /// <summary>
        /// イメージファイルを設定する
        /// </summary>
        /// <param name="id">ID</param>
        /// <param name="filePath">ファイルパス</param>
        public void GButtonImageFile(int id, string filePath)
        {
            if (mGButton == null)
                return;
            if (mGButton.ContainsKey(id)) {
                mGButton[id].setImageFile(filePath);
            }
        }

        /// <summary>
        /// ボタンのタイトルを取得する
        /// </summary>
        /// <param name="id">ID</param>
        /// <returns>タイトル文字列</returns>
        public string GButtonTitleGet(int id)
        {
            if (mGButton == null)
                return "";
            if (mGButton.ContainsKey(id))
                return mGButton[id].mTitle;
            return "";
        }

        /// <summary>
        /// ボタンタイトルのアライメントの設定
        /// </summary>
        /// <param name="id">ID</param>
        /// <param name="ha">水平方向のアライメント</param>
        /// <param name="va">垂直方向のアライメント</param>
        public void GButtonTitleAlignment(int id, HorizontalAlignment ha, VerticalAlignment va)
        {
            if (mGButton == null)
                return;
            if (mGButton.ContainsKey(id)) {
                mGButton[id].mHorizontalAlignment = ha;
                mGButton[id].mVerticalAlignment = va;
            }
        }

        /// <summary>
        /// ボタンの大きさに対するタイトル文字の大きさの比率
        /// </summary>
        /// <param name="id">ID</param>
        /// <param name="ratio">文字大きさの比率</param>
        public void GButtonTitleRatio(int id, double ratio)
        {
            if (mGButton == null)
                return;
            if (mGButton.ContainsKey(id))
                mGButton[id].mTitleRatio = ratio;
        }

        /// <summary>
        /// ボタンのタイトル文字の色設定
        /// </summary>
        /// <param name="id">ID</param>
        /// <param name="color">色</param>
        public void GButtonTitleColor(int id, Brush color)
        {
            if (mGButton == null)
                return;
            if (mGButton.ContainsKey(id))
                mGButton[id].mTitleColor = color;
        }

        /// <summary>
        /// 設定されているボタンのタイトル色を取得
        /// </summary>
        /// <param name="id">ID</param>
        /// <returns>色</returns>
        public Brush GButtonTitleColorGet(int id)
        {
            if (mGButton == null)
                return null;
            if (mGButton.ContainsKey(id))
                return mGButton[id].mTitleColor;
            return null;
        }

        /// <summary>
        /// タイトル文字からボタンのIDを検索する
        /// </summary>
        /// <param name="title">タイトル文字</param>
        /// <returns>ID</returns>
        public int GButtonTitleIdGet(string title)
        {
            if (mGButton == null)
                return -1;
            foreach (KeyValuePair<int, GButton> kvp in mGButton) {
                if (kvp.Value.mTitle.CompareTo(title) == 0) {
                    return kvp.Key;
                }
            }
            return -1;
        }

        /// <summary>
        /// 指定ボタンのBitMapから指定位置のPixcelカラーを取得
        /// </summary>
        /// <param name="id">ID</param>
        /// <param name="x">PixcelのX位置</param>
        /// <param name="y">PixcelのY位置</param>
        /// <returns>カラー値</returns>
        public System.Drawing.Color GButtonGetImagePixcel(int id, int x, int y)
        {
            System.Drawing.Bitmap bitMap = GButtonBitmapGet(id);
            if (bitMap != null) {
                try {
                    return bitMap.GetPixel(x, y);
                } catch (Exception e ) {
                    return System.Drawing.Color.Empty;
                }
            } else
                return System.Drawing.Color.Empty;
        }

        /// <summary>
        /// Bitmapをボタンから取得
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public System.Drawing.Bitmap GButtonBitmapGet(int id)
        {
            if (mGButton == null)
                return null;
            if (mGButton.ContainsKey(id)) {
                //return getBitMapImage(mGButton[id].mImageFile, mGButton[id].mRect);
                return getBitMapImage(mGButton[id].mImageFile, mGButton[id].mTrimmingRect, mGButton[id].mImageSize);
            }
            return null;
        }

        /// <summary>
        /// Bitmapのトリミング後のサイズを取得
        /// </summary>
        /// <param name="id">ID</param>
        /// <returns>Rectサイズ</returns>
        public Rect GButtonSize(int id)
        {
            if (mGButton == null)
                return Rect.Empty;
            if (mGButton.ContainsKey(id)) {
                return mGButton[id].mTrimmingRect;
            }
            return Rect.Empty;
        }

        /// <summary>
        /// BitMapのもとのイメージサイズ(トリミング前)
        /// </summary>
        /// <param name="id">ID</param>
        /// <returns>サイズ</returns>
        public Size GButtonImageSize(int id)
        {
            if (mGButton == null)
                return Size.Empty;
            if (mGButton.ContainsKey(id)) {
                return mGButton[id].mImageSize;
            }
            return Size.Empty;
        }

        /// <summary>
        /// 押されたボタンのIDを取得
        /// </summary>
        /// <param name="pt">押した位置(ワールド座標)</param>
        /// <param name="world">ワールド座標</param>
        /// <returns>ID</returns>
        private int GButtonDownId(Point pt, Box world)
        {
            if (mGButton == null)
                return -1;
            foreach (KeyValuePair<int, GButton> kvp in mGButton) {
                if (kvp.Value.getButtonIn(pt, world)) {
                    return kvp.Key;
                }
            }
            return -1;
        }

        /// <summary>
        /// 押されたボタンのIDを取得
        /// </summary>
        /// <param name="pt">押した位置(ワールド座標)</param>
        /// <returns>ID</returns>
        public int GButtonDownId(Point pt)
        {
            if (mGButton == null)
                return -1;
            if (mGButtonGroup != null) {
                foreach (KeyValuePair<int, GroupGButton> kvp in mGButtonGroup) {
                    for (int i = 0; i < kvp.Value.getSize(); i++) {
                        if (mGButton.ContainsKey(kvp.Key + i))
                            if (mGButton[kvp.Key + i].getButtonIn(pt, mWorld))
                                return kvp.Key + i;
                    }
                }
            }
            foreach (KeyValuePair<int, GButton> kvp in mGButton) {
                if (kvp.Value.getButtonIn(pt, mWorld)) {
                    return kvp.Key;
                }
            }
            return -1;
        }

        /// <summary>
        /// ボタンの状態(押下:down)の設定
        /// </summary>
        /// <param name="id">ID</param>
        /// <param name="down">押下</param>
        public void GButtonDownSet(int id, bool down)
        {
            if (mGButton == null)
                return;
            if (mGButton.ContainsKey(id))
                mGButton[id].mDown = down;
        }

        /// <summary>
        /// ボタンの状態を取得する
        /// </summary>
        /// <param name="id">ID</param>
        /// <returns>押下</returns>
        public bool? GButtonDownGet(int id)
        {
            if (mGButton == null)
                return null;
            if (mGButton.ContainsKey(id))
                return mGButton[id].mDown;
            return null;
        }

        /// <summary>
        /// ボタンの状態(押した)を反転
        /// </summary>
        /// <param name="id">ID</param>
        public void GButtonDwonReversId(int id)
        {
            if (mGButton == null)
                return;
            if (mGButton.ContainsKey(id))
                mGButton[id].mDown = mGButton[id].mDown ? false : true;
        }

        /// <summary>
        /// 全ボタンの押した状態をクリアする
        /// </summary>
        public void GButtonDownClear()
        {
            if (mGButton == null)
                return;
            foreach (KeyValuePair<int, GButton> kvp in mGButton) {
                kvp.Value.mDown = false;
            }
        }

        /// <summary>
        /// 押されているボタンの数を取得
        /// </summary>
        /// <returns>押されているボタンの数</returns>
        public int GButtonDownCount()
        {
            int count = 0;
            if (mGButton == null)
                return 0;
            foreach (KeyValuePair<int, GButton> kvp in mGButton) {
                if (kvp.Value.mDown)
                    count++;
            }
            return count;
        }

        /// <summary>
        /// ボタンの表示/非表示設定
        /// </summary>
        /// <param name="id">ID</param>
        /// <param name="visible">表示(true)/非表示(false)</param>
        public void GButtonVisible(int id, bool visible)
        {
            if (mGButton == null)
                return;
            if (mGButton.ContainsKey(id))
                mGButton[id].mVisible = visible;
        }

        /// <summary>
        /// ボタンの内外判定の有効/無効設定
        /// </summary>
        /// <param name="id">ID</param>
        /// <param name="enable">内外判定</param>
        public void GButtonEnabled(int id, bool enable)
        {
            if (mGButton == null)
                return;
            if (mGButton.ContainsKey(id))
                mGButton[id].mEnabled = enable;
        }
    }


    /// <summary>
    /// ボタンデータ
    /// </summary>
    public class GButton
    {
        public int mId { get; set; } = 0;                           //  ボタンのID
        public BUTTONTYPE mType { get; set; } = BUTTONTYPE.RECT;    //  ボタンの種類
        public Rect mRect { get; set; }                             //  ボタンの大きさ
        public Brush mBorderColor { get; set; } = Brushes.Black;    //  枠の色
        public double mBorderThickness { get; set; } = 1;           //  枠の幅
        public String mTitle { get; set; } = "";                    //  タイトル
        public double mTitleSize { get; set; } = 20;                //  タイトル文字の大きさ
        public double mTitleCircleRatio { get; set; } = 0.7;         //  自動文字サイズの割合
        public Brush mTitleColor { get; set; } = Brushes.Black;     //  タイトル文字の色
        public double mTitleRatio { get; set; } = 1;                //  タイトル文字サイズ比
        public HorizontalAlignment mHorizontalAlignment { get; set; } = HorizontalAlignment.Center;
        public VerticalAlignment mVerticalAlignment { get; set; } = VerticalAlignment.Center;
        private double mMeasureTextRatio;                           //  mesureTextで測定した文字サイズ
        public Brush mBackColor { get; set; } = Brushes.White;      //  背景色
        public Brush mDownBackColor { get; set; } = Brushes.Cyan;   //  ボタンを押した時の背景色
        public bool mDown { get; set; } = false;                    //  ボタンを押された状態
        public bool mVisible = true;                                //  ボタン表示/非表示
        public bool mEnabled { get; set; } = true;                  //  ボタンを有効/無効(内外判定)
        public System.Drawing.Bitmap mResouceFile { get; set; }     //  リソースイメージファイル
        public string mImageFile { get; set; }                      //  イメージファイルパス
        public Rect mTrimmingRect { get; set; }                     //  画像ファイルのトリミング
        public Size mImageSize { get; set; }                        //  仮想の画像ファイルのサイズ

        /// <summary>
        /// ボタンの登録
        /// </summary>
        /// <param name="id">ID</param>
        /// <param name="type">種類</param>
        /// <param name="rect">位置と大きさ</param>
        public GButton(int id, BUTTONTYPE type, Rect rect)
        {
            mId = id;
            mType = type;
            mRect = rect;
        }


        /// <summary>
        /// ボタンタイトルの設定
        /// </summary>
        /// <param name="title">タイトル文字列</param>
        /// <param name="measureTextRatio">ボタンの大きさとの比率</param>
        /// <param name="world">ワールド座標系</param>
        public void setTitle(string title, double measureTextRatio)
        {
            mTitle = title;
            mMeasureTextRatio = measureTextRatio;
            if (mRect.Size.Height / mRect.Size.Width < mMeasureTextRatio) { //  縦長
                mTitleSize = mRect.Size.Height * mTitleRatio;
            } else {                                                        //  横長
                mTitleSize = mRect.Size.Width * mMeasureTextRatio * mTitleRatio * 0.75;
            }
            int n = title.Length - title.Replace("\n".ToString(), "").Length;
            double raito = 0.35;
            mTitleSize /= n + 1 + raito * n;
        }

        /// <summary>
        /// ボタンの内外判定
        /// </summary>
        /// <param name="p">マウスの座標(ワールド座標)</param>
        /// <param name="world">ワールド座標系</param>
        /// <returns>内判定</returns>
        public bool getButtonIn(Point p, Box world)
        {
            if (!mEnabled)
                return false;
            if (mType == BUTTONTYPE.RECT)
                return getRectIn(p, world);
            else if (mType == BUTTONTYPE.CIRCLE) {
                return getCircleIn(p, world);
            }
            return false;
        }

        /// <summary>
        /// ボタンの内外判定(四角)
        /// </summary>
        /// <param name="p">マウスの座標(ワールド座標)</param>
        /// <param name="world">ワールド座標系</param>
        /// <returns>内判定</returns>
        private bool getRectIn(Point pt, Box world)
        {
            double rx = pt.X - ((world.Left - world.Right) < 0 ? mRect.Left : mRect.Left - mRect.Right);
            double ry = pt.Y - ((world.Top - world.Bottom) < 0 ? mRect.Top : mRect.Top - mRect.Height);
            if (0 <= rx && rx <= mRect.Width && 0 <= ry && ry <= mRect.Height)
                return true;
            else
                return false;

        }

        /// <summary>
        /// ボタンの内外判定(楕円)
        /// </summary>
        /// <param name="p">マウスの座標(ワールド座標)</param>
        /// <param name="world">ワールド座標系</param>
        /// <returns>内判定</returns>
        private bool getCircleIn(Point pt, Box world)
        {
            double cx = mRect.Left - Math.Sign(world.Left - world.Right) * mRect.Width / 2.0;
            double cy = mRect.Top - Math.Sign(world.Top - world.Bottom) * mRect.Height / 2.0;
            double r = mRect.Height / 2.0;
            double dx = (pt.X - cx) * mRect.Height / mRect.Width;
            double dy = pt.Y - cy;
            if (r * r < (dx * dx + dy * dy))
                return false;
            else
                return true;
        }

        /// <summary>
        /// イメージリソースファイルを設定
        /// </summary>
        /// <param name="bitmap">リソースファイル</param>
        public void setResourceFile(System.Drawing.Bitmap bitmap)
        {
            mResouceFile = bitmap;
            mImageFile = null;
        }

        /// <summary>
        /// イメージファイルの設定
        /// </summary>
        /// <param name="filePath">イメージファイルパス</param>
        public void setImageFile(string filePath)
        {
            mImageFile = filePath;
            mResouceFile = null;
        }
    }
}
