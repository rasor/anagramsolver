﻿using anagramsolver.containers;
using anagramsolver.models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace anagramsolver.services
{
    public class LoopSetsOf4WordsHelper<TCurrentSetOfXPos> : LoopSetsBase<TCurrentSetOfXPos> where TCurrentSetOfXPos : CurrentSetOf4Pos, new()
    {
        public LoopSetsOf4WordsHelper(ConsoleLogger logger, MD5 Md5HashComputer,
            IAnagramContainer AnagramCtrl, IWordlistContainer WordlistCtrl) :
            base(logger.ConsoleWriteLine, Md5HashComputer, AnagramCtrl, WordlistCtrl)
        { }

        protected override int LoopWordCombinationsInCurrentSet(int numberOfJackpots, TCurrentSetOfXPos currentSetLength, ref ulong combinationCounter, ref ulong subsetCounter)
        {
            // Create list with permutations for string.Format: "{0} {1} {2} {3}" from [0,1,2,3] to [3,2,1,0] = 24 permutations
            string[] listOfWordPermutationsReplacementString = PermutationsCreator.CreateListOfWordPermutationsReplacementStrings(4);

            var tableByWordLength = _wordlistCtrl.TableByWordLength;
            var listOfPointersToWord4 = tableByWordLength[currentSetLength.Word4Length];
            var listOfPointersToWord3 = tableByWordLength[currentSetLength.Word3Length];
            var listOfPointersToWord2 = tableByWordLength[currentSetLength.Word2Length];
            var listOfPointersToWord1 = tableByWordLength[currentSetLength.Word1Length];

            ulong currentSetCombinations = (ulong)(listOfPointersToWord1.Count * listOfPointersToWord2.Count * listOfPointersToWord3.Count * listOfPointersToWord4.Count);
            _consoleWriteLine(" Combinations: " + string.Format("{0:n0}", combinationCounter) + ". Subsets: " + string.Format("{0:n0}", subsetCounter) + ". NextSet: " + currentSetLength.ToString() + " having " + string.Format("{0:n0}", currentSetCombinations) + " combinations");

            // List to avoid checking same sentence twice
            HashSet<int[]> uniqueListOfSentencesHavingWordsWithSameLength = new HashSet<int[]>(new ArrayComparer());
            ulong uniqueListOfSentencesHavingWordsWithSameLengthCounter = 0;
            ulong skippedChecksCounter = 0;

            // Since we know that there won't be any long words before len = 11, then we make the outer loop pass those 0 values first
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
                            var rows = new int[][] { word1Row, word2Row, word3Row, word4Row };
                            var combinedWordToValidate = CombineRows(rows);
                            var isSubset = _anagramCtrl.IsSubset(combinedWordToValidate);

                            // Do MD5 check if the two words combined is still a subset of anagram
                            bool gotJackpot = false;
                            if (isSubset)
                            {
                                subsetCounter++;
                                // Put words in a list, so they can be passed on as a collection
                                int[] currentSentence = new int[] { word1Pointer, word2Pointer, word3Pointer, word4Pointer };

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
            if (uniqueListOfSentencesHavingWordsWithSameLengthCounter > 0)
            {
                _consoleWriteLine("  UniqueListOfSentencesHavingWordsWithSameLength: " + uniqueListOfSentencesHavingWordsWithSameLengthCounter + ". SkippedChecks: " + skippedChecksCounter);
            }
            return numberOfJackpots;
        }
    }
}
