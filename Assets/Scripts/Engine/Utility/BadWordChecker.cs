// 如果定义了SMART_CHECK_SEPARATER，则会智能判断敏感词中杂志的符号和空格
#define SMART_CHECK_SEPARATER
//#define POOR_MEMORY

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections;

/// <summary>
/// 脏字检查和过滤
/// </summary>
public class BadWordChecker {

    private readonly Node root_ = new Node();
    private object locker_ = new object();

#if SMART_CHECK_SEPARATER

    /// <summary>
    /// 在检索的时候应该被忽略的字符
    /// </summary>
    private static class IgnoreChars {
        readonly static char[] chars_ = new char[] {
            '~', '`', '!', '@', '#', '$', '%', '^', '&', '*', '(', ')', '-', '=', '_', '+', '{', '}', '[', ']', '|', '\\', '/', '?', '<', '>', ',', '.', ':', ';',
            '\'', '"'
        };

        static IgnoreChars() {
            Array.Sort(chars_);
        }

        public static bool Include(char ch) {
            if (ch < 0x7f) {
                if (ch <= ' ') {
                    return true;
                }
                return (Array.BinarySearch<char>(chars_, ch) >= 0);
            }
            if (ch >= 0x7f && ch <= 0xA0) {
                return true;
            } else {
                return false;
            }
        }
    }

#endif

    public BadWordChecker() { }

    public BadWordChecker(IEnumerable<string> keywords) {
        if (keywords != null) {
            foreach (string keyword in keywords) {
                if (!string.IsNullOrEmpty(keyword)) {
                    this.AddBadWord(keyword);
                }
            }
        }
    }

    public BadWordChecker(Stream input) {
        if (input != null) {
            using (TextReader reader = new StreamReader(input)) {
                while (true) {
                    string keyword = reader.ReadLine();
                    if (keyword == null) {
                        break;
                    } else {
                        this.AddBadWord(keyword);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 加入一个脏字串
    /// </summary>
    /// <param name="badWord">不为null的关键字</param>
    public void AddBadWord(string badWord) {
#if SMART_CHECK_SEPARATER
        StringBuilder sb = new StringBuilder(badWord.Length);
        foreach (char ch in badWord) {
            if (!IgnoreChars.Include(ch)) {
                sb.Append(ch);
            }
        }
        if (sb.Length != badWord.Length) {
            badWord = sb.ToString();
        }
#endif
        if (badWord.Length > 0) {
            lock (locker_) {
                root_.AddChild(badWord, 0);
            }
        }
    }

    /// <summary>
    /// 判断给定字串s是否含有关键词
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public bool Match(string s) {
        if (!string.IsNullOrEmpty(s)) {
            for (int i = 0; i < s.Length; ++i) {
                lock (this.locker_) {
                    if (root_.Match(s, i)) {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    /// <summary>
    /// 将字串中的关键字替换成*号
    /// （服务器不用这段代码，不用保证线程安全）
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public string Replace(string s) {
        return Replace(s, '*');
    }

    /// <summary>
    /// 将字串中的关键字替换成指定字符
    /// （服务器不用这段代码，不用保证线程安全）
    /// </summary>
    /// <param name="s"></param>
    /// <param name="replaced"></param>
    /// <returns></returns>
    public string Replace(string s, char replaceChar) {
        if (string.IsNullOrEmpty(s)) {
            return string.Empty;
        }
        char[] chars = null;
        for (int i = 0; i < s.Length; ) {
            int p = root_.MatchGreedy(s, i);
            if (p > i) {
                // 找到最长匹配了，替换
                if (chars == null) {
                    chars = s.ToCharArray();
                }
                for (int n = i; n < p; ++n) {
#if SMART_CHECK_SEPARATER
                    if (!IgnoreChars.Include(chars[n])) {
                        chars[n] = replaceChar;
                    }
#else
                    chars[n] = replaceChar;
#endif
                }
                i = p;
            } else {
                ++i;
            }
        }
        //
        if (chars != null) {
            return new string(chars);
        } else {
            return s;
        }
    }

	public void TrimExcess() {
		root_.TrimExcess();
	}

    /// <summary>
    /// 节点
    /// </summary>
	class Node {
        private bool isEnd_;                        // 本节点是否为某个（较短）关键字的结尾
#if POOR_MEMORY
        SortedList<char, Node>
#else
		Dictionary<char, Node>
#endif
		children_;   // 后继子节点

        /// <summary>
        /// 加入一个子序列串，非线程安全，只在初始化的时候用
        /// </summary>
        /// <param name="s">关键字</param>
        /// <param name="pos">位置</param>
        public void AddChild(string s, int pos) {
            if (pos >= s.Length) {
                this.isEnd_ = true; // 无论还有没有后继子节点，本节点总是一个关键字的结尾
            } else {
                if (children_ == null) {
                    children_ =
#if POOR_MEMORY
						new SortedList<char, Node>();
#else
						new Dictionary<char, Node>();
#endif
                }
                char ch = char.ToLower(s[pos]);
                Node child;
                if (!children_.TryGetValue(ch, out child)) {
                    child = new Node();
                    children_.Add(ch, child);
                }
                child.AddChild(s, pos + 1);
            }
        }

        public bool Match(string s, int pos) {
            // 本结点是叶结点，或是一个终点
            if (this.isEnd_ || children_ == null) {
                return true;
            }
            // 源串已结束，说明不匹配
            if (pos == s.Length) {
                return false;
            }
            // 源串未结束，找后续字符
            char ch = char.ToLower(s[pos]);
            Node node;
            lock (children_) {
                if (!children_.TryGetValue(ch, out node)) {
                    node = null;
                }
            }
            if (node != null) {
                // 后续字符找到了，继续
                return node.Match(s, pos + 1);
            } else {
                // 后续字符不匹配（比如关键词是ABC，则ABD不能匹配）
#if SMART_CHECK_SEPARATER
                if (IgnoreChars.Include(ch)) {
                    return Match(s, pos + 1);
                }
#endif
                return false;
            }
        }

        /// <summary>
        /// 贪婪式匹配，线程安全
        /// </summary>
        /// <param name="s">要判断的字符串</param>
        /// <param name="pos">从字符串的哪个位置开始判断</param>
        /// <returns>返回最长匹配点的下一个位置</returns>
        public int MatchGreedy(string s, int pos) {
            // 如果源串已结束，则
            // 如果本节点是终点，则匹配，否则不匹配
            if (pos == s.Length) {
                // 如果本节点是终点，匹配！否则不匹配
                return this.isEnd_ ? pos : -1; //         alreadyMatchLength: alreadyMatchLength - 1;
            }
            // 没有后续字符了（本结点是叶结点）
            // 说明已经达到最长匹配了
            if (children_ == null) {
                return pos;
            }
            // 源串未结束，找后续字符
            char ch = s[pos];
#if SMART_CHECK_SEPARATER
            if (IgnoreChars.Include(ch)) {
                return MatchGreedy(s, pos + 1);
            }
#endif
            ch = char.ToLower(ch);
            //
            Node node;
            lock (children_) {
                if (!children_.TryGetValue(ch, out node)) {
                    node = null;
                }
            }
            if (node != null) {
                // 后续字符找到了，继续
                int p = node.MatchGreedy(s, pos + 1);
                if (p > pos) {
                    return p;
                } else {
                    if (this.isEnd_) {
                        return pos;
                    } else {
                        return -1;
                    }
                }
            } else {
                // 后续字符不匹配
                return this.isEnd_ ? pos : -1;
            }
        }

		public void TrimExcess() {
			if (this.children_ != null) {
				foreach (Node child in this.children_.Values) {
					child.TrimExcess();
				}
#if POOR_MEMORY
				this.children_.TrimExcess();
#else
				this.children_ = new Dictionary<char, Node>(this.children_);
#endif
			}
		}

#if DEBUG
        public int GetChildrenCount() {
            if (this.children_ == null) {
                return 0;
            } else {
                int r = children_.Count;
                foreach (Node n in children_.Values) {
                    r += n.GetChildrenCount();
                }
                return r;
            }
        }
#endif
    }

#if DEBUG
    public int GetChildrenCount() {
        return root_.GetChildrenCount();
    }
#endif
}

class InvalidNameList {

#if UNIT_TEST
    public
#else
    private
#endif
    class InvalidCharsRange {
        public class Exception : System.Exception {
            public Exception(string line)
                :base(string.Format("Load Invalid-Chars-Range failed: \"{0}\"", line))
            { }
        }

        private struct Range : IComparable<Range> {
            public readonly char Low;
            public readonly char High;
            public Range(char low, char high) {
                if (low <= high) {
                    this.Low = low;
                    this.High = high;
                } else {
                    this.Low = high;
                    this.High = high;
                }
            }
            public bool IsInRange(char ch) {
                return ch >= this.Low && ch <= this.High;
            }
            public int CompareTo(Range other) {
                int r = this.Low - other.Low;
                if (r == 0) {
                    return this.High - other.High;
                } else {
                    return r;
                }
            }
            public override string ToString() {
                return string.Format("(Low={0}, High={1})", this.Low, this.High);
            }
        }
        private List<Range> rangeList_;
        private readonly object locker_ = new object();

        private static void ThrowException(Range r1, Range r2) {
            throw new Exception(string.Format("({0} and {1})", r1, r2));
        }

        public void LoadFromReader(TextReader reader) {
            List<Range> list = new List<Range>(20);
            while (true) {
                string line = reader.ReadLine();
                if (line == null) { break; }
                line = line.Trim();
                if (line.Length == 0) { continue; } // 空行
                if (line[0] == '#') { continue; }   // 注释
                string[] valueStr = line.Split('-');
                if (valueStr.Length != 2) {
                    throw new Exception(line);
                }
                char low = GetCharValueFromString(valueStr[0]);
                char high = GetCharValueFromString(valueStr[1]);
                list.Add(new Range(low, high));
            }
            if (list.Count == 0) {
                throw new Exception("Empty list");
            }
            list.Sort();
            for (int i = list.Count - 1; i > 0; --i) {
                Range r2 = list[i];
                Range r1 = list[i - 1];
                if (r2.Low <= r1.High) {
                    throw new Exception(string.Format("Cross range ({0} and {1})", r1, r2));
                }
            }
            //
            lock (locker_) {
                this.rangeList_ = list;
            }
        }

        public void LoadFromStream(Stream input) {
            LoadFromReader(new StreamReader(input));
        }

        private static char GetCharValueFromString(string s) {
            uint u = 0;
            foreach (char ch in s) {
                u <<= 4;
                if (ch >= '0' && ch <= '9') {
                    u |= (uint)(ch - '0');
                } else if (ch >= 'a' && ch <= 'f') {
                    u |= (uint)(ch - 'a' + 10);
                } else if (ch >= 'A' && ch <= 'F') {
                    u |= (uint)(ch - 'A' + 10);
                } else {
                    throw new Exception(s);
                }
            }
            if (u > char.MaxValue) {
                throw new Exception(s);
            }
            return (char)u;
        }

        /// <summary>
        /// 用二分法对给定char进行判断，看它是否落在“非法字符区间范围”内
        /// </summary>
        private static bool IsInRange(char ch, List<Range> list) {
            int count = list.Count;
            int lowIndex = 0;
            while (count > 0) {
                int count2 = count / 2;
                int middIndex = lowIndex + count2;
                Range r = list[middIndex];
                if (ch >= r.Low) {
                    if (ch <= r.High) { return true; }
                    // ch 落在 midd 的左边
                    lowIndex = middIndex + 1;
                    count -= count2 + 1;
                } else {
                    // ch 落在 midd 的右边
                    count = count2;
                }
            }
            return false;
        }

        public bool IsInRange(char ch) {
            List<Range> list;
            lock (locker_) {
                list = rangeList_;
            }
            if (list != null && list.Count > 0) {
                return IsInRange(ch, list);
            } else {
                return false;
            }
        }

        public bool IsInRange(string s) {
            List<Range> list;
            lock (locker_) {
                list = rangeList_;
            }
            if (list != null && list.Count > 0) { 
                foreach (char ch in s) {
                    if (IsInRange(ch, list)) {
                        return true;
                    }
                }
            }
            return false;
        }

		public void TrimExcess() {
			lock (locker_) {
				if (this.rangeList_ != null) {
					this.rangeList_.TrimExcess();
				}
			}
		}
    }

    private readonly InvalidCharsRange invalidCharsRange_ = new InvalidCharsRange();

	private readonly HashSet<string> entireMatch_ = new HashSet<string>();

	private readonly List<Regex> regexList_ = new List<Regex>();

    private InvalidNameList() { }

    public static readonly InvalidNameList Null = new InvalidNameList();

	public InvalidNameList(Stream entireWordMatchInput, Stream regexpMatchInput, Stream invalidCharsRangeInput) {
		using (TextReader reader = new StreamReader(entireWordMatchInput)) {
			while (true) {
				string word = reader.ReadLine();
				if (word == null) {
					break;
				} else {
					word = word.Trim().ToLower();
					if (word.Length > 0) {
						entireMatch_.Add(word.Trim().ToLower());
					}
				}
			}
		}
		using (TextReader reader = new StreamReader(regexpMatchInput)) {
			while (true) {
				string pattern = reader.ReadLine();
				if (pattern == null) {
					break;
				} else {
					pattern = pattern.Trim();
					if (pattern.Length > 0) {
						Regex regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);//| RegexOptions.Compiled 
						regexList_.Add(regex);
					}
				}
			}
		}
        //
        this.invalidCharsRange_.LoadFromStream(invalidCharsRangeInput);
	}

	/// <summary>
	/// 判断给定字符对于角色名来说是否合法，是否整字匹配EntireMatch，或者匹配给定的正则表达式
	/// 保证线程安全
	/// </summary>
	/// <param name="s"></param>
	/// <returns></returns>
	public bool Contains(string s) {
        // 先判断是不是所有字符都合法
        if (this.invalidCharsRange_.IsInRange(s)) {
            return true;
        }
        // 转成小写，然后再看是不是有整字匹配的
		s = s.ToLower();
		// 按MSDN的文档，HashSet所有成员方法都不是线程安全的
		lock (entireMatch_) {
			if (entireMatch_.Contains(s)) {
				return true;
			}
		}
		// 按MSDN的文档，List的所成员方法都不是线程安全的
		// 所以Copy一份出来
		Regex[] regexArray = null;
		lock (regexList_) {
			regexArray = regexList_.ToArray();
		}
		// MSDN说：Regex的方法是线程安全的
		if (regexArray != null) {
			foreach (Regex regex in regexArray) {
				if (regex.IsMatch(s)) {
					return true;
				}
			}
		}
		return false;
	}

	public void TrimExcess() {
		this.invalidCharsRange_.TrimExcess();
		this.entireMatch_.TrimExcess();
		this.regexList_.TrimExcess();
	}
}
