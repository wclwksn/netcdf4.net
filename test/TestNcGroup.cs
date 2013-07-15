/*
 * Author Luke Campbell <LCampbell@ASAScience.com>
 * netcdf3.test.TestNcGroup
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using netcdf4;

namespace netcdf4.test {
    class TestNcGroup : UnitTest {
        protected const string filePath = "nc_clobber.nc";
        public TestNcGroup() {
            AddTest(test_get_name, "test_get_name");
            AddTest(test_get_parent_group, "test_get_parent_group");
            AddTest(test_get_group_count, "test_get_group_count");
            AddTest(test_get_groups, "test_get_groups");
            AddTest(TestVlen, "TestVlen");
            AddTest(TestOpaque, "TestOpaque");
            AddTest(TestEnum, "TestEnum");
        }

        public NcFile newFile(string filePath) {
            return TestHelper.NewFile(filePath);
        }


        public bool test_get_name() {
            string groupName;
            int id;
            CheckDelete(filePath);
            NcFile file=null;
            NcGroup group;
            try {
                file = newFile(filePath);
                id = file.GetId();
                Assert.NotEquals(id,0);
                group = file;
                groupName = group.GetName();
                Assert.Equals(groupName, "/");
                Assert.True(group.IsRootGroup());
            } finally {
                file.Close();
            }
            CheckDelete(filePath);
            return true;
        }
        public bool test_get_parent_group() {
            NcGroup group;
            NcFile file=null;

            group = new NcGroup();
            try {
                NcGroup grpTest = group.GetParentGroup();
                throw new AssertFailedException("NcNullGrp not thrown");
            } catch (exceptions.NcNullGrp e) {
            } catch (Exception e) {
                throw new AssertFailedException("NcNullGrp not thrown");
            }

            try {
                file = newFile(filePath);
                group = file.GetParentGroup();
                Assert.True(group.IsNull());
            } finally {
                file.Close();
            }
            return true;
        }

        public bool test_get_group_count() {
            NcGroup group;
            NcFile file=null;
            try {
                file = newFile(filePath);
                group = file;
                int groupCount;
                int oGroupCount=0;
                groupCount = group.GetGroupCount(GroupLocation.AllGrps);
                Assert.Equals(groupCount, 1); // Only the root group/file so no groups are defined
                NcGroup childGroup = group.AddGroup("child1");
                Assert.Equals(group.GetGroupCount(GroupLocation.AllGrps), 2);
                Dictionary<string, NcGroup> groups = group.GetGroups(GroupLocation.AllGrps);
                for(int i=0;i<groups.Count;i++) {
                    KeyValuePair<string, NcGroup> k = groups.ElementAt(i);
                    if(i==0)
                        Assert.Equals(k.Key, "/");
                    else if(i==1)
                        Assert.Equals(k.Key, "child1");
                }

            } finally {
                file.Close();
            }

            CheckDelete(filePath);
            return true;
        }
        public bool test_get_groups() {
            NcGroup group;
            NcFile file=null;

            NcGroup child1_1;
            NcGroup child1_2;

            try {
                file = newFile(filePath);
                group = file;
                /* -- Check differentiability for GetGroup -- */
                child1_1 = file.AddGroup("group1");
                file.AddGroup("group2");

                group = file.GetGroup("group1");
                Assert.Equals(group.GetName(), "group1");
                /* -- Check that sets work for GetGroups -- */
                child1_2 = child1_1.AddGroup("group1"); // Second one named group1
                HashSet<NcGroup> groups = file.GetGroups("group1");
                foreach(NcGroup g in groups) {
                    if(g.GetId() != child1_1.GetId() && g.GetId() != child1_2.GetId()) {
                        throw new AssertFailedException("Incorrect group in set");
                    }
                }
            } finally {
                file.Close();
            }

            CheckDelete(filePath);
            return true;
        }
        public bool TestVlen() {
            NcFile file = null;
            NcVlenType vlen = null;
            NcDim dim = null;
            NcVar var = null;
            string stringVlenBuffer = "hi there";
            string stringReadBuffer;

            int iLen=0;

            sbyte[] sbyteVlenBuffer = new sbyte[] { 0, -12, -4 };
            sbyte[] sbyteReadBuffer = new sbyte[8];
            
            byte[] byteVlenBuffer = new byte[] { 0, 12, 4 };
            byte[] byteReadBuffer = new byte[8];
            
            Int16[] Int16VlenBuffer = new Int16[] { 0, -12, -4 };
            Int16[] Int16ReadBuffer = new Int16[8];
            
            UInt16[] UInt16VlenBuffer = new UInt16[] { 0, 12, 4 };
            UInt16[] UInt16ReadBuffer = new UInt16[8];
            
            Int32[] Int32VlenBuffer = new Int32[] { 0, -12, -4 };
            Int32[] Int32ReadBuffer = new Int32[8];
            
            UInt32[] UInt32VlenBuffer = new UInt32[] { 0, 12, 4 };
            UInt32[] UInt32ReadBuffer = new UInt32[8];
            
            Int64[] Int64VlenBuffer = new Int64[] { 0, -12, -4 };
            Int64[] Int64ReadBuffer = new Int64[8];
            
            UInt64[] UInt64VlenBuffer = new UInt64[] { 0, 12, 4 };
            UInt64[] UInt64ReadBuffer = new UInt64[8];
            
            float[] floatVlenBuffer = new float[] { 0, 12, 4 };
            float[] floatReadBuffer = new float[8];
            
            double[] doubleVlenBuffer = new double[] { 0, 12, 4 };
            double[] doubleReadBuffer = new double[8];

            try {
                file = TestHelper.NewFile(filePath);
                dim = file.AddDim("time", 1);

                // string
                vlen = file.AddVlenType("vlenstring", NcChar.Instance);
                var = file.AddVar("string", vlen, dim);
                var.PutVar(new Int32[] { 0 }, stringVlenBuffer );
                var.GetVar(new Int32[] { 0 }, out stringReadBuffer );
                Assert.Equals(stringVlenBuffer, stringReadBuffer);
                
                // sbyte
                vlen = file.AddVlenType("vlensbyte", NcByte.Instance);
                var = file.AddVar("sbyte", vlen, dim);
                var.PutVar(new Int32[] { 0 }, sbyteVlenBuffer );
                iLen=var.GetVar(new Int32[] { 0 }, sbyteReadBuffer );
                for(int i=0; i<iLen; i++)
                    Assert.Equals(sbyteVlenBuffer[i], sbyteReadBuffer[i]);
                
                // byte
                vlen = file.AddVlenType("vlenbyte", NcByte.Instance);
                var = file.AddVar("byte", vlen, dim);
                var.PutVar(new Int32[] { 0 }, byteVlenBuffer );
                iLen=var.GetVar(new Int32[] { 0 }, byteReadBuffer );
                for(int i=0; i<iLen; i++)
                    Assert.Equals(byteVlenBuffer[i], byteReadBuffer[i]);
                
                // Int16
                vlen = file.AddVlenType("vlenInt16", NcShort.Instance);
                var = file.AddVar("Int16", vlen, dim);
                var.PutVar(new Int32[] { 0 }, Int16VlenBuffer );
                iLen=var.GetVar(new Int32[] { 0 }, Int16ReadBuffer );
                for(int i=0; i<iLen; i++)
                    Assert.Equals(Int16VlenBuffer[i], Int16ReadBuffer[i]);
                
                // UInt16
                vlen = file.AddVlenType("vlenUInt16", NcUshort.Instance);
                var = file.AddVar("UInt16", vlen, dim);
                var.PutVar(new Int32[] { 0 }, UInt16VlenBuffer );
                iLen=var.GetVar(new Int32[] { 0 }, UInt16ReadBuffer );
                for(int i=0; i<iLen; i++)
                    Assert.Equals(UInt16VlenBuffer[i], UInt16ReadBuffer[i]);
                
                // Int32
                vlen = file.AddVlenType("vlenInt32", NcInt.Instance);
                var = file.AddVar("Int32", vlen, dim);
                var.PutVar(new Int32[] { 0 }, Int32VlenBuffer );
                iLen=var.GetVar(new Int32[] { 0 }, Int32ReadBuffer );
                for(int i=0; i<iLen; i++)
                    Assert.Equals(Int32VlenBuffer[i], Int32ReadBuffer[i]);
                
                // UInt32
                vlen = file.AddVlenType("vlenUInt32", NcUint.Instance);
                var = file.AddVar("UInt32", vlen, dim);
                var.PutVar(new Int32[] { 0 }, UInt32VlenBuffer );
                iLen=var.GetVar(new Int32[] { 0 }, UInt32ReadBuffer );
                for(int i=0; i<iLen; i++)
                    Assert.Equals(UInt32VlenBuffer[i], UInt32ReadBuffer[i]);
                
                // Int64
                vlen = file.AddVlenType("vlenInt64", NcInt64.Instance);
                var = file.AddVar("Int64", vlen, dim);
                var.PutVar(new Int32[] { 0 }, Int64VlenBuffer );
                iLen=var.GetVar(new Int32[] { 0 }, Int64ReadBuffer );
                for(int i=0; i<iLen; i++)
                    Assert.Equals(Int64VlenBuffer[i], Int64ReadBuffer[i]);
                
                // UInt64
                vlen = file.AddVlenType("vlenUInt64", NcUint64.Instance);
                var = file.AddVar("UInt64", vlen, dim);
                var.PutVar(new Int32[] { 0 }, UInt64VlenBuffer );
                iLen=var.GetVar(new Int32[] { 0 }, UInt64ReadBuffer );
                for(int i=0; i<iLen; i++)
                    Assert.Equals(UInt64VlenBuffer[i], UInt64ReadBuffer[i]);
                
                // float
                vlen = file.AddVlenType("vlenfloat", NcFloat.Instance);
                var = file.AddVar("float", vlen, dim);
                var.PutVar(new Int32[] { 0 }, floatVlenBuffer );
                iLen=var.GetVar(new Int32[] { 0 }, floatReadBuffer );
                for(int i=0; i<iLen; i++)
                    Assert.Equals(floatVlenBuffer[i], floatReadBuffer[i]);
                
                // double
                vlen = file.AddVlenType("vlendouble", NcDouble.Instance);
                var = file.AddVar("double", vlen, dim);
                var.PutVar(new Int32[] { 0 }, doubleVlenBuffer );
                iLen=var.GetVar(new Int32[] { 0 }, doubleReadBuffer );
                for(int i=0;i<iLen;i++)
                    Assert.Equals(doubleVlenBuffer[i], doubleReadBuffer[i]);


            } finally {
                file.Close();
            }
            CheckDelete(filePath);
            return true;
        }
        public bool TestOpaque() {
            NcFile file = null;
            NcVar var = null;
            NcDim dim = null;
            NcType type = null;
            NcOpaqueType opaqueType = null;

            byte[] opaqueBuffer = new byte[32];
            byte[] readBuffer = new byte[32];
            for(int i=0;i<32;i++) opaqueBuffer[i] = (byte)i;

            try {
                file = TestHelper.NewFile(filePath);
                type = file.AddOpaqueType("opaque", 32);
                opaqueType = new NcOpaqueType(type);

                Assert.Equals(type.GetTypeClass(), NcTypeEnum.NC_OPAQUE);
                Assert.Equals(opaqueType.GetTypeSize(), 32);

                dim = file.AddDim("time", 1);
                var = file.AddVar("opaqueVar", opaqueType, dim);
                int iLen = 0;
                var.PutVar(new Int32[] { 0 }, opaqueBuffer);
                iLen = var.GetVar(new Int32[] { 0 }, readBuffer);
                Assert.Equals(iLen, 32);
                for(int i=0;i<32;i++)
                    Assert.Equals(readBuffer[i], opaqueBuffer[i]);


            } finally {
                file.Close();
            }
            CheckDelete(filePath);
            return true;
        }
        public bool TestEnum() {
            NcFile file = null;
            NcType type = null;
            NcEnumType enumType = null;
            NcDim dim = null;
            NcVar var = null;
            sbyte[] sbyteBuffer = new sbyte[1];
            byte[] byteBuffer = new byte[1];
            Int16[] Int16Buffer = new Int16[1];
            UInt16[] UInt16Buffer = new UInt16[1];
            Int32[] Int32Buffer = new Int32[1];
            UInt32[] UInt32Buffer = new UInt32[1];
            Int64[] Int64Buffer = new Int64[1];
            UInt64[] UInt64Buffer = new UInt64[1];
            
            try {
                file = TestHelper.NewFile(filePath);
                dim = file.AddDim("time", 1);
                
                type = file.AddEnumType("sbyteenum", NcEnumType.Types.NC_BYTE);
                enumType = new NcEnumType(type);
                Assert.Equals(enumType.GetName(), "sbyteenum");
                Assert.Equals(enumType.GetMemberCount(), 0);
                Assert.True(NcByte.Instance.Equals(enumType.GetBaseType()));
                enumType.AddMember("BASE", 0);
                enumType.AddMember("VOR", 1);
                enumType.AddMember("DME", 2);
                enumType.AddMember("TAC", 3);

                var = file.AddVar("enumsbyteVar", enumType, dim);
                var.PutVar(new sbyte[] { 3 });
                var.GetVar(sbyteBuffer);
                Assert.Equals(sbyteBuffer[0], (sbyte)3);

                type = file.AddEnumType("byteenum", NcEnumType.Types.NC_UBYTE);
                enumType = new NcEnumType(type);
                Assert.Equals(enumType.GetName(), "byteenum");
                Assert.Equals(enumType.GetMemberCount(), 0);
                Assert.True(NcUbyte.Instance.Equals(enumType.GetBaseType()));
                enumType.AddMember("BASE", 0);
                enumType.AddMember("VOR", 1);
                enumType.AddMember("DME", 2);
                enumType.AddMember("TAC", 3);

                var = file.AddVar("enumbyteVar", enumType, dim);
                var.PutVar(new byte[] { 3 });
                var.GetVar(byteBuffer);
                Assert.Equals(byteBuffer[0], (byte)3);

                type = file.AddEnumType("Int16enum", NcEnumType.Types.NC_SHORT);
                enumType = new NcEnumType(type);
                Assert.Equals(enumType.GetName(), "Int16enum");
                Assert.Equals(enumType.GetMemberCount(), 0);
                Assert.True(NcShort.Instance.Equals(enumType.GetBaseType()));
                enumType.AddMember("BASE", 0);
                enumType.AddMember("VOR", 1);
                enumType.AddMember("DME", 2);
                enumType.AddMember("TAC", 3);

                var = file.AddVar("enumInt16Var", enumType, dim);
                var.PutVar(new Int16[] { 3 });
                var.GetVar(Int16Buffer);
                Assert.Equals(Int16Buffer[0], (Int16)3);

                type = file.AddEnumType("UInt16enum", NcEnumType.Types.NC_USHORT);
                enumType = new NcEnumType(type);
                Assert.Equals(enumType.GetName(), "UInt16enum");
                Assert.Equals(enumType.GetMemberCount(), 0);
                Assert.True(NcUshort.Instance.Equals(enumType.GetBaseType()));
                enumType.AddMember("BASE", 0);
                enumType.AddMember("VOR", 1);
                enumType.AddMember("DME", 2);
                enumType.AddMember("TAC", 3);

                var = file.AddVar("enumUInt16Var", enumType, dim);
                var.PutVar(new UInt16[] { 3 });
                var.GetVar(UInt16Buffer);
                Assert.Equals(UInt16Buffer[0], (UInt16)3);

                type = file.AddEnumType("Int32enum", NcEnumType.Types.NC_INT);
                enumType = new NcEnumType(type);
                Assert.Equals(enumType.GetName(), "Int32enum");
                Assert.Equals(enumType.GetMemberCount(), 0);
                Assert.True(NcInt.Instance.Equals(enumType.GetBaseType()));
                enumType.AddMember("BASE", 0);
                enumType.AddMember("VOR", 1);
                enumType.AddMember("DME", 2);
                enumType.AddMember("TAC", 3);

                var = file.AddVar("enumInt32Var", enumType, dim);
                var.PutVar(new Int32[] { 3 });
                var.GetVar(Int32Buffer);
                Assert.Equals(Int32Buffer[0], (Int32)3);

                type = file.AddEnumType("UInt32enum", NcEnumType.Types.NC_UINT);
                enumType = new NcEnumType(type);
                Assert.Equals(enumType.GetName(), "UInt32enum");
                Assert.Equals(enumType.GetMemberCount(), 0);
                Assert.True(NcUint.Instance.Equals(enumType.GetBaseType()));
                enumType.AddMember("BASE", 0);
                enumType.AddMember("VOR", 1);
                enumType.AddMember("DME", 2);
                enumType.AddMember("TAC", 3);

                var = file.AddVar("enumUInt32Var", enumType, dim);
                var.PutVar(new UInt32[] { 3 });
                var.GetVar(UInt32Buffer);
                Assert.Equals(UInt32Buffer[0], (UInt32)3);

                type = file.AddEnumType("Int64enum", NcEnumType.Types.NC_INT64);
                enumType = new NcEnumType(type);
                Assert.Equals(enumType.GetName(), "Int64enum");
                Assert.Equals(enumType.GetMemberCount(), 0);
                Assert.True(NcInt64.Instance.Equals(enumType.GetBaseType()));
                enumType.AddMember("BASE", (Int64)0);
                enumType.AddMember("VOR", (Int64)1);
                enumType.AddMember("DME", (Int64)2);
                enumType.AddMember("TAC", (Int64)3);
                Assert.Equals(enumType.GetMemberCount(), 4);
                Assert.Equals(enumType.GetMemberNameFromValue( (sbyte) 1), "VOR");
                Assert.Equals(enumType.GetMemberNameFromValue( (byte) 1), "VOR");
                Assert.Equals(enumType.GetMemberNameFromValue( (Int16) 1), "VOR");
                Assert.Equals(enumType.GetMemberNameFromValue( (UInt16) 1), "VOR");
                Assert.Equals(enumType.GetMemberNameFromValue( (Int32) 1), "VOR");
                Assert.Equals(enumType.GetMemberNameFromValue( (UInt32) 1), "VOR");
                Assert.Equals(enumType.GetMemberNameFromValue( (Int64) 1), "VOR");
                Assert.Equals(enumType.GetMemberNameFromValue( (UInt64) 1), "VOR");

                var = file.AddVar("enumInt64Var", enumType, dim);
                var.PutVar(new Int64[] { 3 });
                var.GetVar(Int64Buffer);
                Assert.Equals(Int64Buffer[0], (Int64)3);

                type = file.AddEnumType("UInt64enum", NcEnumType.Types.NC_UINT64);
                enumType = new NcEnumType(type);
                Assert.Equals(enumType.GetName(), "UInt64enum");
                Assert.Equals(enumType.GetMemberCount(), 0);
                Assert.True(NcUint64.Instance.Equals(enumType.GetBaseType()));
                enumType.AddMember("BASE", 0);
                enumType.AddMember("VOR", 1);
                enumType.AddMember("DME", 2);
                enumType.AddMember("TAC", 3);

                var = file.AddVar("enumUInt64Var", enumType, dim);
                var.PutVar(new UInt64[] { 3 });
                var.GetVar(UInt64Buffer);
                Assert.Equals(UInt64Buffer[0], (UInt64)3);

                


            } finally {
                file.Close();
            }
            CheckDelete(filePath);
            return true;
        }
    }
}

