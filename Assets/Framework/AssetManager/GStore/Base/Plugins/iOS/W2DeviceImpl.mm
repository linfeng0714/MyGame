
// Converts C style string to NSString
NSString* CreateNSString (const char* string)
{
    if (string)
        return [NSString stringWithUTF8String: string];
    else
        return [NSString stringWithUTF8String: ""];
}

// Helper method to create C string copy
char* MakeStringCopy2 (const char* string)
{
    if (string == NULL)
        return NULL;
    
    char* res = (char*)malloc(strlen(string) + 1);
    strcpy(res, string);
    return res;
}

#import <Foundation/Foundation.h>
#import <sys/utsname.h>
// When native code plugin is implemented in .mm / .cpp file, then functions
// should be surrounded with extern "C" block to conform C function naming rules
extern "C" {
	
	const char* W2DeviceLanguageName()
	{
        NSArray * lan_s=[NSLocale preferredLanguages];
        
        if(lan_s == nil || lan_s.count <=0)
        {
            return nil;
        }
        
        NSString * c_l=[lan_s objectAtIndex:0];
        return MakeStringCopy2([c_l UTF8String]);
	}
    
    //禁止设备自动锁屏
    void _SetIdleTimerDisabled()
    {
        [[UIApplication sharedApplication] setIdleTimerDisabled:YES];
    }

	//获取设备可用空间
	long long _GetFreeStorage()
    {
        NSDictionary *fattributes = [[NSFileManager defaultManager] fileSystemAttributesAtPath:NSHomeDirectory()];
        NSNumber *num = [fattributes objectForKey:NSFileSystemFreeSize];
        return [num longLongValue];
    }

	//iOS避免文件被同步到iCloud或iTunes
	bool _AddSkipBackupAttributeToItemAtPath(const char* filePathString)
	{
		NSString* path = [NSString stringWithUTF8String: filePathString];
		NSURL* URL= [NSURL fileURLWithPath: path];
		assert([[NSFileManager defaultManager] fileExistsAtPath: [URL path]]);
 
		NSError *error = nil;
		BOOL success = [URL setResourceValue: [NSNumber numberWithBool: YES]
									  forKey: NSURLIsExcludedFromBackupKey error: &error];
		if(!success){
			NSLog(@"Error excluding %@ from backup %@", [URL lastPathComponent], error);
		}
		return success;
	}
    
    bool _GetPhoneXModel()
    {
        BOOL iphoneX = false;
        struct utsname systemInfo;
        uname(&systemInfo);
        NSString *deviceModel = [NSString stringWithCString:systemInfo.machine encoding:NSUTF8StringEncoding];
        
        if([deviceModel isEqualToString:@"iPhone10,3"] || [deviceModel isEqualToString:@"iPhone10,6"]){
            iphoneX = true;
        }
        return iphoneX;
    }
}

