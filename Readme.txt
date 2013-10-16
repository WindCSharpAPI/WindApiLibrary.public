				Wind .Net Api 说明
1.文件夹说明
API:WindApi C++ 接口文件

lib:WindApi C++ lib文件

WindOriginalApiLibrary: CLI/C++  实现Variant->.net object的转换

WindApiLibrary: C#接口

TestWindApi: C#接口测试

2.说明

WindOriginalApiLibrary 的存在本意是使用C++.NET实现.NET接口，但是随着包装的进行觉得代码过于冗余，繁复，于是只保留了上面的Variant->.net object的转换的功能。
对于SAFEARRAY由于时间比较急，也出于效率的考虑，使用了泛型的一维数组装载SAFEARRAY的多维数组，使用的时候稍微要计算下，这个可以参看TestApi
其他的代码没有删除，而是作为代码垃圾场，说不定以后我能从里面翻到什么疙瘩。（主要烦人的地方时Managed Codes<->Native Codes的互相调用引起的各种问题，格式，生存周期等）


WindApiLibrary 是对外的最终接口代码，使用了类来封装，但是没有做CancelRequest（这个其实比较容易，因为没有需要，所以没有做）,和其他一些函数（也是没有需求）

代码总体写的比较匆忙，一些地方没有写注释，一些变量的命名也没有特别明确。但是作为一个工具已经在线上跑了大概1个月，这个接口没有出现过大的问题。希望能够帮到你。




										沈迪
										银河期货
										2013.10.10
