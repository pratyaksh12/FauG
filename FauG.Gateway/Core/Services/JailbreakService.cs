using System;
using System.Text.RegularExpressions;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.ML.Tokenizers;
namespace FauG.Gateway.Core.Services;

/*
    This class is responsible for loading the model from the base-path.
    It has one job that is to ckeck for prompt-injection/Jailbreaking in a 32k input token window
    Uses IDisposable to dispose of the model architecture once the program is canceled
    Uses ONNX runtime for a faster loading of models and to keep the language native.
*/
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

        // tranform raw prompts to tokens
        var encodeIds = _tokenizer.EncodeToIds(prompt);
        var tokenIds = encodeIds.Take(512).Select(t => (long)t).ToArray();

        // match padding with ONNX expectations in case of a smaller prompt
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

        // run the model
        using var results = _session.Run(inputs);

        // extract the results
        var outputTensor = results.First().AsTensor<float>();
        var safeScore = outputTensor[0, 0];
        var injectionScore = outputTensor[0, 1];

        return injectionScore > safeScore;
    }
    
    public void Dispose()
    {
        _session!.Dispose();
    }
}
