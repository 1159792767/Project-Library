import unittest
from auto_release import stable_version, choose_version, fingerprint

class ResolutionTests(unittest.TestCase):
    def test_stable_only(self):
        self.assertEqual(choose_version('1.0.1', ['1.0.2','2.0.0-preview.1','1.1.0']), '1.1.0')
    def test_no_downgrade(self):
        self.assertEqual(choose_version('3.0.0', ['1.0.1']), '3.0.0')
    def test_unsupported_current_is_explicit(self):
        for value in ['1.*', '$(CommonVersion)', '[1.0,2.0)', '1.0.0-beta']:
            with self.assertRaises(ValueError): stable_version(value)
    def test_change_detection(self):
        self.assertEqual(fingerprint('abc', {'b':'1.0.0','a':'1.0.0'}), fingerprint('abc', {'a':'1.0.0','b':'1.0.0'}))
        self.assertNotEqual(fingerprint('abc', {'a':'1.0.0'}), fingerprint('abc', {'a':'1.0.1'}))
        self.assertNotEqual(fingerprint('abc', {}), fingerprint('def', {}))

if __name__ == '__main__': unittest.main()
