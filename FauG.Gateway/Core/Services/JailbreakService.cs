using System;
using System.Text.RegularExpressions;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.ML.Tokenizers;

namespace FauG.Gateway.Core.Services;

public class JailbreakService : IDisposable
{
    private readonly InferenceSession? _session;
    private readonly Tokenizer? _tokenizer;

    public JailbreakService()
    {
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "Models", "sentinel-onnx");
        var modelPath = Path.Combine(basePath, "model.onnx");
        var vocabPath = Path.Combine(basePath, "vocab.json");
        var mergePath = Path.Combine(basePath, "merges.txt");

        if(File.Exists(modelPath) && File.Exists(vocabPath) && File.Exists(mergePath))
        {
            _session = new InferenceSession(modelPath);
            _tokenizer = BpeTokenizer.Create(vocabPath, mergePath);
        }
        else
        {
            Console.WriteLine($"Jailbreak model not found at {basePath}");
        }
    }

    public bool IsJailBreak(string prompt)
    {
        if(_session is null || _tokenizer is null) return false;

        var encodeIds = _tokenizer.EncodeToIds(prompt);
        var tokenIds = encodeIds.Take(512).Select(t => (long)t).ToArray();

        // match padding with ONNX expectations
        var paddedTokens = new long[512];
        var attentionMask = new long[512];

        for(int i = 0; i < tokenIds.Length; i++)
        {
            paddedTokens[i] = tokenIds[i];
            attentionMask[i] = 1;
        }

        // Create Tensors
        var inputTensors = new DenseTensor<long>(paddedTokens, [1, 512]);
        var maskTensors = new DenseTensor<long>(paddedTokens, [1, 512]);

        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor("input_ids", inputTensors),
            NamedOnnxValue.CreateFromTensor("attention_mask", maskTensors)
        };

        using var results = _session.Run(inputs);
        var outputTensor = results.First().AsTensor<float>();

        var safeScore = outputTensor[0, 0];
        var injectionScore = outputTensor[0, 1];

        return injectionScore > safeScore;
    }
    
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}
