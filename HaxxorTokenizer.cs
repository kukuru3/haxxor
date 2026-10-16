using System;
using System.Collections.Generic;
using System.Text;

using UnityEngine;

namespace Haxxor {
    class HaxxorTokenizer {
        StringBuilder tokenizerSB = new StringBuilder();
        TokenizerStates tokenizerState;
        readonly ICommandInstantiator commandCreator;
        private readonly HaxxorComputer computer;

        public HaxxorTokenizer(ICommandInstantiator cmdCreator, HaxxorComputer computer) {
            this.commandCreator = cmdCreator;
            this.computer = computer;
        }

        enum TokenizerStates {
            ReadingText,
            ReadingCommandToken,
            ReadingLiteral,
        }

        private const char SYMBOL_CMD_START = '{';
        private const char SYMBOL_CMD_END = '}';
        private const char SYMBOL_NEWLINE = '\n';
        private const char SYMBOL_CMD_DELIMITER = ':';
        private const string LITERAL_START = "##>";
        private const string LITERAL_END = "<##";

        internal IEnumerable<HaxxorToken> Tokenize(string rawText) {
            tokenizerSB.Clear();
            for (var c = 0; c < rawText.Length; c++) {
                var @char = rawText[c];
                if (@char == SYMBOL_NEWLINE) {
                    if (tokenizerState == TokenizerStates.ReadingText) {
                        if (tokenizerSB.Length > 0) yield return FinishCurrentTokenAndStartNextOne();
                        yield return new HaxNewLine();
                    } else if (tokenizerState == TokenizerStates.ReadingLiteral) {
                        if (tokenizerSB.Length > 0) yield return FinishCurrentTokenAndStartNextOne();
                        yield return new HaxNewLine();
                        // do nothing
                    }
                } else if (@char == SYMBOL_CMD_START) {
                    switch (tokenizerState) {
                        case TokenizerStates.ReadingCommandToken:
                            throw new InvalidOperationException("Cannot process nested tokens because the developer is a talentless hack");
                        case TokenizerStates.ReadingText:
                            if (tokenizerSB.Length > 0) yield return FinishCurrentTokenAndStartNextOne();
                            tokenizerState = TokenizerStates.ReadingCommandToken;
                            tokenizerSB.Append(@char);
                            break;
                        case TokenizerStates.ReadingLiteral:
                            tokenizerSB.Append(@char);
                            break;
                    }
                    
                } else if (@char == SYMBOL_CMD_END) {
                    switch (tokenizerState) {
                        case TokenizerStates.ReadingCommandToken:
                            tokenizerSB.Append(@char);
                            if (tokenizerSB.Length > 0) yield return FinishCurrentTokenAndStartNextOne();
                            tokenizerState = TokenizerStates.ReadingText;
                            break;
                        case TokenizerStates.ReadingText:
                            throw new InvalidOperationException("closing brace without opening brace");
                        case TokenizerStates.ReadingLiteral:
                            tokenizerSB.Append(@char);
                            break;
                    }
                    

                } else {
                    tokenizerSB.Append(@char);

                    if (tokenizerSB.Length > 2) { 
                        if  (tokenizerSB[tokenizerSB.Length - 1] == '>'
                          && tokenizerSB[tokenizerSB.Length - 2] == '#'
                          && tokenizerSB[tokenizerSB.Length - 3] == '#') {  // begin literal
                            tokenizerSB.Remove(tokenizerSB.Length - 3, 3);
                            if (tokenizerSB.Length > 0) yield return FinishCurrentTokenAndStartNextOne();
                            tokenizerState = TokenizerStates.ReadingLiteral;
                        } else if (tokenizerSB[tokenizerSB.Length - 1] == '#'
                          && tokenizerSB[tokenizerSB.Length - 2] == '#'
                          && tokenizerSB[tokenizerSB.Length - 3] == '<') {
                            tokenizerSB.Remove(tokenizerSB.Length - 3, 3);
                            if (tokenizerSB.Length > 0) yield return FinishCurrentTokenAndStartNextOne();
                            tokenizerState = TokenizerStates.ReadingText;
                        }
                    }
                }
            }
            if (tokenizerSB.Length > 0)
                yield return FinishCurrentTokenAndStartNextOne();
        }

        
        private void EndLiteral() => throw new NotImplementedException();

        private HaxxorToken FinishCurrentTokenAndStartNextOne() {
            if (tokenizerState == TokenizerStates.ReadingLiteral) {
                var token = new HaxText(tokenizerSB.ToString());
                tokenizerSB.Clear();
                return token;
            } else {
                var str = tokenizerSB.ToString();
                var token = CompileToken(str);
                tokenizerSB.Clear();
                return token;
            }
        }

        internal HaxxorToken CompileToken(string txt) {
            var token = DoCompileToken(txt);
            token.Inject(computer.bus);
            return token;
        }

        HaxxorToken DoCompileToken(string tokenString) {
            if (string.IsNullOrEmpty(tokenString)) throw new ArgumentException("Cannot tokenize empty string");
            if (tokenString[0] == SYMBOL_CMD_START && tokenString[tokenString.Length - 1] == SYMBOL_CMD_END) {
                tokenString = tokenString.Substring(1, tokenString.Length - 2);

                var items = tokenString.Split(SYMBOL_CMD_DELIMITER);
                var cmdIdentifier = items[0];
                var cmdInstance = this.commandCreator.CreateCommand(cmdIdentifier);
                if (cmdInstance == null) {
                    Debug.LogWarning($"Unknown command : {cmdIdentifier}, will skip token");
                    return new HaxText($"<< INVALID COMMAND : {cmdIdentifier}>>");
                }
                if (items.Length > 1) {
                    if (cmdInstance is ICommandWithParams acceptsParams) { 
                        var paramsArr = new string[items.Length - 1];
                        for (var i = 0; i < paramsArr.Length; i++) paramsArr[i] = items[i + 1];
                        acceptsParams.AcceptParams(paramsArr);
                    } else {
                        Debug.LogWarning($"Parameters provided for command {cmdIdentifier}, but it does not implement {nameof(ICommandWithParams)}");
                    }
                }
                return cmdInstance;
            } else if (tokenString[0] == '\n' && tokenString.Length == 1) {
                return new HaxNewLine();
            } else
                return new HaxText(tokenString);
        }
    }
}