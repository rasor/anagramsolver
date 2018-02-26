﻿using anagramsolver.containers;
using anagramsolver.models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace anagramsolver.services
{
    public class LoopSetsOf5WordsHelper<TCurrentSetOfXPos> : LoopSetsBase<TCurrentSetOfXPos> where TCurrentSetOfXPos : CurrentSetOf5Pos, new()
    {
        public LoopSetsOf5WordsHelper(ConsoleLogger logger, MD5 Md5HashComputer,
            IAnagramContainer AnagramCtrl, IWordlistContainer WordlistCtrl) :
            base(logger.ConsoleWriteLine, Md5HashComputer, AnagramCtrl, WordlistCtrl)
        { }

        protected override int LoopWordCombinationsInCurrentSet(int numberOfJackpots, TCurrentSetOfXPos currentSetLength, ref ulong combinationCounter, ref ulong subsetCounter)
        {
            // Create list with permutations for string.Format: "{0} {1} {2} {3} {4}" from [0,1,2,3,4] to [4,3,2,1,0] = 120 permutations
            string[] listOfWordPermutationsReplacementString = PermutationsCreator.CreateListOfWordPermutationsReplacementStrings(5);

            var tableByWordLength = _wordlistCtrl.TableByWordLength;
            var listOfPointersToWord5 = tableByWordLength[currentSetLength.DictOfWordLengths[5] - 1];
            var listOfPointersToWord4 = tableByWordLength[currentSetLength.DictOfWordLengths[4] - 1];
            var listOfPointersToWord3 = tableByWordLength[currentSetLength.DictOfWordLengths[3] - 1];
            var listOfPointersToWord2 = tableByWordLength[currentSetLength.DictOfWordLengths[2] - 1];
            var listOfPointersToWord1 = tableByWordLength[currentSetLength.DictOfWordLengths[1] - 1];

            ulong currentSetCombinations = (ulong)(listOfPointersToWord1.Count * listOfPointersToWord2.Count * listOfPointersToWord3.Count * listOfPointersToWord4.Count * listOfPointersToWord5.Count);
            _consoleWriteLine(" Combinations: " + string.Format("{0:n0}", combinationCounter) + ". Subsets: " + string.Format("{0:n0}", subsetCounter) + ". NextSet: " + currentSetLength.ToString() + " having " + string.Format("{0:n0}", currentSetCombinations) + " combinations");

            // List to avoid checking same sentence twice
            HashSet<int[]> uniqueListOfSentencesHavingWordsWithSameLength = new HashSet<int[]>(new ArrayComparer());
            ulong uniqueListOfSentencesHavingWordsWithSameLengthCounter = 0;
            ulong skippedChecksCounter = 0;

            // Since we know that there won't be any long words before len = 11, then we make the outer loop pass those 0 values first
            foreach (var word5Pointer in listOfPointersToWord5)
            {
                foreach (var word4Pointer in listOfPointersToWord4)
                {
                    foreach (var word3Pointer in listOfPointersToWord3)
                    {
                        foreach (var word2Pointer in listOfPointersToWord2)
                        {
                            foreach (var word1Pointer in listOfPointersToWord1)
                            {
                                // ConsoleWriteLine(" Combinations: " + combinationCounter + ". Subsets: " + subsetCounter);

                                var word1Row = _wordlistCtrl.TableFilter2_WordMatrix[word1Pointer];
                                var word2Row = _wordlistCtrl.TableFilter2_WordMatrix[word2Pointer];
                                var word3Row = _wordlistCtrl.TableFilter2_WordMatrix[word3Pointer];
                                var word4Row = _wordlistCtrl.TableFilter2_WordMatrix[word4Pointer];
                                var word5Row = _wordlistCtrl.TableFilter2_WordMatrix[word5Pointer];
                                var rows = new int[][] { word1Row, word2Row, word3Row, word4Row, word5Row };
                                var combinedWordToValidate = CombineRows(rows);
                                var isSubset = _anagramCtrl.IsSubset(combinedWordToValidate);

                                // Do MD5 check if the two words combined is still a subset of anagram
                                bool gotJackpot = false;
                                if (isSubset)
                                {
                                    subsetCounter++;
                                    // Put words in a list, so they can be passed on as a collection
                                    int[] currentSentence = new int[] { word1Pointer, word2Pointer, word3Pointer, word4Pointer, word5Pointer };

                                    // Now that we are down to the few sentences that are also subsets, then we'll keep them in an ordered unique list,
                                    // So those sentences having same words are not checked more than once
                                    //if (currentSetLength.AnyOfSameLength)
                                    //{
                                    //    Array.Sort(currentSentence);
                                    //    // If we don't have that sentence, then do md5Check
                                    //    if (!uniqueListOfSentencesHavingWordsWithSameLength.Contains(currentSentence))
                                    //    {
                                    //        uniqueListOfSentencesHavingWordsWithSameLengthCounter++;
                                    //        uniqueListOfSentencesHavingWordsWithSameLength.Add(currentSentence);

                                    //        gotJackpot = FetchWordsAndCheckMd5(ref numberOfJackpots, currentSentence, listOfWordPermutationsReplacementString);
                                    //    }
                                    //    else
                                    //    {
                                    //        skippedChecksCounter++;
                                    //    }
                                    //}
                                    //// No words of same lenght, so just do check
                                    //else
                                    {
                                        gotJackpot = FetchWordsAndCheckMd5RemoveFoundHash(ref numberOfJackpots, currentSentence, listOfWordPermutationsReplacementString);
                                    }
                                }
                                combinationCounter++;
                            }
                        }
                    }
                }
            }
            if (uniqueListOfSentencesHavingWordsWithSameLengthCounter > 0)
            {
                _consoleWriteLine("  UniqueListOfSentencesHavingWordsWithSameLength: " + uniqueListOfSentencesHavingWordsWithSameLengthCounter + ". SkippedChecks: " + skippedChecksCounter);
            }
            return numberOfJackpots;
        }
    }
}
