using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Media;

namespace 匹配游戏
{
    public partial class Form1 : Form
    {
        //计时变量
        int time = 0;
        //采用相对地址的方式定义音乐变量：backGroundMusic、matchMusic、notMatchMusic、victoryMusic、loseMusic
        SoundPlayer backGroundMusic = new SoundPlayer(Application.StartupPath + @"\backGroundMusic.wav");
        SoundPlayer matchMusic = new SoundPlayer(Application.StartupPath + @"\matchMusic.wav");
        SoundPlayer notMatchMusic = new SoundPlayer(Application.StartupPath + @"\notMatchMusic.wav");
        SoundPlayer victoryMusic = new SoundPlayer(Application.StartupPath + @"\victoryMusic.wav");
        SoundPlayer loseMusic = new SoundPlayer(Application.StartupPath + @"\loseMusic.wav");

        //两个引用变量，跟踪第一次和第二次分别单击了那个Label控件
        Label firstClicked = null;//指向第一个Label控件
        Label secondClicked = null;//指向第二个Label控件

        //使用Random对象来选择随机图标
        Random random = new Random();

        //使用list对象来存放图标字母，每个
        //每个字母对应于Webdings 字体的一个图标，每个图标在列表中出现两侧
        List<string> icons = new List<string>()
        {
           "!", "!", "N", "N", ",", ",", "k", "k","h","h",
            "b", "b", "v", "v", "w", "w", "z","z","j","j",
            "l", "l", "a", "a", "q", "q", "e","e","r","r",
            "x", "x", "c", "c", "m", "m"           
        };

        /// <summary>
        /// 向窗体中的Label控件随机分配图标
        /// 使用foreach关键字
        /// </summary>
        /// 
        private void AssignIconsToSquares()
        {
            //List对象有16个元素，而游戏中有36个空格，故每个元素对应一个空格
            foreach (Control control in tableLayoutPanel1.Controls)//获取包含在TableLayoutPanel内的控件的集合
            {
                Label iconLabel = control as Label;
                if (iconLabel != null)
                {
                    int randomNumber = random.Next(icons.Count);//获取List对象icons中实际的因素个数
                    iconLabel.Text = icons[randomNumber];
                    iconLabel.ForeColor = tableLayoutPanel1.BackColor;//Color.Black;//设置图标颜色
                    icons.RemoveAt(randomNumber);//移除randomNumber所在的元素
                }
            }
        }

        public Form1()
        {
            InitializeComponent();//初始化控件
            AssignIconsToSquares();//调用新的方法以在现实之前对自身进行设置

            //游戏开始时播放背景音乐
            backGroundMusic.Play();
            timer3.Start();//开启全局计时
        }

        private void Form1_Load(object sender, EventArgs e)
        {
          
        }
        /// <summary>
        /// 当玩家单击其中一个带有隐藏图标的方块时，程序通过将
        /// 图标的颜色更改为黑色来向玩家显示该图标
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void label_Click(object sender, EventArgs e)
        {
            //只有当两个不匹配的图标出现时才计时，所以在计时器工作时要忽视任何点击
            if (timer1.Enabled == true )
                return;

            //否则，点击有效
            Label clickedLabel = sender as Label;//定义专门的Label控件

            if (clickedLabel != null)//检查是否已将 “clickedLabel”成功从对象转换（强制转换）为 Label 控件
            {
                //如果该图标已经是黑色的，说明用户已经点击过了，则忽视该次点击
                if (clickedLabel.ForeColor == Color.Black)
                    return;

                //否则，将图标显示为黑色
                if (firstClicked == null)
                {
                    firstClicked = clickedLabel;
                    firstClicked.ForeColor = Color.Black;
                    timer2.Start();//当点击第一个图标，定时器2开始计时
                    return;
                }
                secondClicked = clickedLabel;
                secondClicked.ForeColor = Color.Black;
                timer1.Start();
                timer2.Stop();

                //调用CheckForWinner()程序，检查玩家是否胜利
                CheckForWinner();

                //检查两个图标是否匹配，若匹配，则保持图标颜色并重置firstClicked和secondClicked
                if (firstClicked.Text == secondClicked.Text)
                {
                   //播放音乐
                    matchMusic.Play();

                    //重置firstClicked和secondClicked
                    firstClicked = null;
                    secondClicked = null;

                    //关闭定时器1和定时器2
                    timer1.Stop();
                    timer2.Stop();
                    return;
                }
            }
        }

        /// <summary>
        /// 当玩家单击两个没有匹配的图标时开始计时，计时500ms后自动停止，并隐藏这两个图标
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            //停止计时
            timer1.Stop();

            //音乐
            notMatchMusic.Play();

            //隐藏前一个图标
            firstClicked.ForeColor = firstClicked.BackColor;

            //重置firstClicked和secondClicked
            firstClicked = secondClicked;
            
            timer2.Start();

            secondClicked = null;
            
        }
      
        /// <summary>
        /// 如果玩家太慢，并且没有及时单击第二个图标，可以通过隐藏第一个图标使游戏变得更具挑战性。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer2_Tick(object sender, EventArgs e)
        {
            timer2.Stop();
           // timer1.Stop();
            if (secondClicked == null )
            {
                //匹配失败音乐
                notMatchMusic.Play();

                //隐藏前一个图标
                firstClicked.ForeColor = firstClicked.BackColor;

                //重置firstClicked
                firstClicked = null;
                
                return;
            }
        }

        /// <summary>
        /// 验证玩家是否获胜。
        /// 通过检查每个图标的文本颜色与背景颜色是否相同来检查每个图标看是否匹配，
        /// 不同为匹配，相同则不匹配       
        /// </summary>
        private void CheckForWinner()
        {
            int minus = 0;
            int seconds = 0;
            //foreach遍历每个图标
            foreach (Control control in tableLayoutPanel1.Controls)//获取控件内的集合元素
            {
                Label iconLabel = control as Label;

                if (iconLabel != null)
                {
                    if (iconLabel.ForeColor == iconLabel.BackColor)
                        return;
                }
            }
            
            //停止计时
            timer1.Stop();
            timer2.Stop();
            timer3.Stop();

            //胜利音乐
            victoryMusic.PlayLooping();

            minus = time / 600;
            seconds = (time %600 )/10;
            //如果没有跳出循环，则说明没有找到不匹配的图标，说明玩家赢了
            MessageBox.Show("恭喜您，成功通过游戏。您所用时为："+minus.ToString()+"分"+seconds.ToString()+"秒");
            Close();//结束游戏
        }

        private void timer3_Tick(object sender, EventArgs e)
        {
            time++;
        }

    }
}
