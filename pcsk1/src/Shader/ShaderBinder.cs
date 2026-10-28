using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace pcsk1.src.Shader
{
    public sealed class ShaderBinder
    {
        private readonly Effect _shader;

        private readonly Dictionary<string, EffectParameter>
            _parameters = new();

        public ShaderBinder(Effect shader)
        {
            _shader = shader ?? throw new ArgumentNullException(nameof(shader));

            foreach (EffectParameter parameter in shader.Parameters)
            {
                _parameters.Add(parameter.Name, parameter);
            }
        }

        public Effect Shader => _shader;

        private EffectParameter GetParameter(string name)
        {
            if (!_parameters.TryGetValue(name, out var parameter))
            {
                throw new ArgumentException(
                    $"Uniform '{name}' não encontrado no shader."
                );
            }

            return parameter;
        }

        public ShaderBinder SetUniform(string name, float value)
        {
            GetParameter(name).SetValue(value);
            return this;
        }

        public ShaderBinder SetUniform(string name, int value)
        {
            GetParameter(name).SetValue(value);
            return this;
        }

        public ShaderBinder SetUniform(string name, Vector2 value)
        {
            GetParameter(name).SetValue(value);
            return this;
        }


        public ShaderBinder SetUniform(string name, Vector3 value)
        {
            GetParameter(name).SetValue(value);
            return this;
        }

        public ShaderBinder SetUniform(string name, Vector4 value)
        {
            GetParameter(name).SetValue(value);
            return this;
        }

        public ShaderBinder SetUniform(string name, Matrix value)
        {
            GetParameter(name).SetValue(value);
            return this;
        }

        public ShaderBinder SetUniform(string name, Texture2D value)
        {
            GetParameter(name).SetValue(value);
            return this;
        }

        public ShaderBinder And(string name, float value)
            => SetUniform(name, value);

        public ShaderBinder And(string name, int value)
            => SetUniform(name, value);

        public ShaderBinder And(string name, Vector2 value)
            => SetUniform(name, value);

        public ShaderBinder And(string name, Vector3 value)
            => SetUniform(name, value);

        public ShaderBinder And(string name, Vector4 value)
            => SetUniform(name, value);

        public ShaderBinder And(string name, Matrix value)
            => SetUniform(name, value);

        public ShaderBinder And(string name, Texture2D value)
            => SetUniform(name, value);
    }
}
