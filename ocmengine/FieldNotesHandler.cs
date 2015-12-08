// 
//  Copyright 2011  Kyle Campbell
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace ocmengine
{

	public class FieldNotesHandler
	{		
		private enum FieldState { Code,Date,Type,Comment};
		private enum MessageState { Start,InQuotes,OutQuotes };

		public static void WriteToFile(CacheLog log, String fnFile)
		{
			List<CacheLog> logs = new List<CacheLog>();
			logs.Add(log);
			WriteToFile(logs, fnFile);
			
		}
		
		
		public static void WriteToFile(List<CacheLog> logs, String fnFile)
		{
			FileStream fstream = new FileStream(fnFile, FileMode.Append);
			TextWriter writer = new StreamWriter(fstream, Encoding.Unicode);
			foreach (CacheLog log in logs)
			{
				log.WriteToFieldNotesFile(writer);
			}
			writer.Close();
			fstream.Close();
		}
		
		public static void ClearFieldNotes(String fnFile)
		{
			if (File.Exists(fnFile))
			    File.Delete(fnFile);
		}
		                           
		
		public static List<CacheLog> GetLogs(String fnFile, String OwnerId)
		{
			FieldState fieldState = FieldState.Code;
			MessageState messageState = MessageState.Start;

			List<CacheLog> logs = new List<CacheLog>();
			StringBuilder code = new StringBuilder();
			StringBuilder logDate = new StringBuilder();
			StringBuilder logStatus = new StringBuilder();
			StringBuilder logMessage = new StringBuilder();
			FileStream fstream = File.OpenRead(fnFile);
			TextReader reader = new StreamReader(fnFile, Encoding.Unicode);
			while (reader.Peek() > 0)
			{
				int c = reader.Read();
				switch( fieldState )
				{
					case FieldState.Code:
						if (c == ',')
						{
							fieldState = FieldState.Date;
						}
						else if (c == '\n' || c=='\r') 
						{
							code.Length = 0;
							logDate.Length = 0;
							logStatus.Length = 0;
							logMessage.Length = 0;
							fieldState = FieldState.Code;
						}
						else
						{
							code.Append((char)c);
						}
						break;
					case FieldState.Date:
						if (c == ',')
						{
							fieldState = FieldState.Type;
						}
						else if (c == '\n' || c == '\r')
						{
							code.Length = 0;
							logDate.Length = 0;
							logStatus.Length = 0;
							logMessage.Length = 0;
							fieldState = FieldState.Code;
						}
						else
						{
							logDate.Append((char)c);
						}
						break;
					case FieldState.Type:
						if (c == ',')
						{
							fieldState = FieldState.Comment;
							messageState = MessageState.Start;
						}
						else if (c == '\n' || c == '\r') 
						{
							code.Length = 0;
							logDate.Length = 0;
							logStatus.Length = 0;
							logMessage.Length = 0;
							fieldState = FieldState.Code;
						}
						else
						{
							logStatus.Append((char)c);
						}
						break;
					case FieldState.Comment:
						switch( messageState ) 
						{
							case MessageState.Start:
								if( c == '"' )
								{
									messageState = MessageState.InQuotes;
								}
								else
								{
									reader.ReadLine();
									code.Length = 0;
									logDate.Length = 0;
									logStatus.Length = 0;
									logMessage.Length = 0;
									fieldState = FieldState.Code;
								}
								break;
							case MessageState.InQuotes:
								if (c == '"')
								{
									messageState = MessageState.OutQuotes;
								}
								else if (c != '\r')
								{
									logMessage.Append((char)c);
								}
								break;
							case MessageState.OutQuotes:
								if (c == '"')
								{
									logMessage.Append((char)c);
									messageState = MessageState.InQuotes;
								}
								else if (c=='\n' || c=='\r')
								{
									CacheLog log = new CacheLog();
									log.CacheCode = code.ToString();
									log.LogDate = DateTime.Parse(logDate.ToString());
									log.LogStatus = logStatus.ToString();
									log.LogMessage = logMessage.ToString();
									log.LogKey = log.CacheCode + log.LogDate.ToFileTime().ToString();
									log.LoggedBy = "OCM";
									log.FinderID = OwnerId;
									logs.Add(log);

									code.Length = 0;
									logDate.Length = 0;
									logStatus.Length = 0;
									logMessage.Length = 0;
									fieldState = FieldState.Code;
								}
								else
								{
									code.Length = 0;
									logDate.Length = 0;
									logStatus.Length = 0;
									logMessage.Length = 0;
									fieldState = FieldState.Code;
								}
								break;
						}
						break;
				}
			}
			reader.Close();
			fstream.Close();
			return logs;
		}
		
		private static void RebuildLogMessage (string[] parts)
		{
			StringBuilder builder = new StringBuilder();
			for (int i=3; i < parts.Length; i++)
			{
				if (i>3)
					builder.Append(",");
				builder.Append(parts[i]);
			}
			parts[3] = builder.ToString();
		}
	}
}
