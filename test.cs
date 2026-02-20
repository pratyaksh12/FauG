using System;
using System.IO;
using Microsoft.ML.Tokenizers;

class Program {
    static void Main() {
        var t = typeof(Tokenizer);
        foreach (var m in t.GetMethods()) {
            Console.WriteLine(m.Name);
        }
    }
}
